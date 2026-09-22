using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;

namespace SchoolAccount.CollectNotifications.Services;

public class StatusChangedLedgerMonitoringService(
    StatusChangedLedgerMonitoringServiceInstrumentation log,
    ILastRanService lastRanService,
    ILedgerStore ledgerStore,
    IGovNotifyService govNotifyService
)
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var runningAt = DateTime.UtcNow;

        var lastRan = await lastRanService.GetTimestampAsync(cancellationToken);

        if (lastRan.IsFailure)
        {
            log.LastRunDateUnavailable(lastRan.Error);
            return;
        }

        log.RunStarted(lastRan.Value, runningAt);

        if (lastRan.Value == LastRanService.NeverRun)
        {
            log.NoPreviousRunRecorded(runningAt);

            var seeded = await lastRanService.SetTimestampAsync(runningAt, cancellationToken);

            if (seeded.IsFailure)
            {
                log.CouldNotRecordFirstRun(seeded.Error);
            }

            return;
        }

        var changes = await ledgerStore.GetWhatHasChangedAsync(lastRan.Value, true, cancellationToken);

        if (changes.IsFailure)
        {
            log.LedgerUnavailable(changes.Error);
            return;
        }

        log.ChangesFound(changes.Value.Count);

        // The query already pairs each change with its registered recipients, so a school with two
        // registered contacts arrives here as two changes.
        var whatToNotify = changes.Value
            .Select(change => new Notification(
                change.LaeStab,
                change.Email,
                change.ReturnStatusCode.GetHumanName(),
                change.SchoolName))
            .ToList();

        var timestampUpdate = await lastRanService.SetTimestampAsync(runningAt, cancellationToken);

        if (timestampUpdate.IsFailure)
        {
            log.CouldNotRecordRun(timestampUpdate.Error);
            return;
        }

        foreach (var notify in whatToNotify)
        {
            cancellationToken.ThrowIfCancellationRequested();
            log.NotifyingRecipient(notify.Recipient, notify.LaeStab);

            Result<NotificationResult> result;

            try
            {
                result = await govNotifyService.SendMessage(
                    GovNotifyTemplates.CensusStatusChange,
                    notify.Recipient,
                    new Dictionary<string, dynamic>
                    {
                        { "status", notify.Status },
                        { "school_name", notify.School }
                    });
            }
            catch (Exception exception)
            {
                // The watermark has already moved, so giving up on the whole run here would lose
                // every notification after this one. Carry on and let the rest through.
                log.NotificationFailed(exception, notify.Recipient);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                log.NotificationRejected(result.Error);
            }

            // Anything the Notify service reports as a failure rather than a warning is a problem
            // with the whole run, a rate limit or a bad key, so there is no point working through
            // the rest.
            if (result.IsFailure)
            {
                log.SendingStopped(whatToNotify.Count - whatToNotify.IndexOf(notify) - 1);
                break;
            }
        }

        log.RunFinished((DateTime.UtcNow - runningAt).TotalMilliseconds);
    }
}
