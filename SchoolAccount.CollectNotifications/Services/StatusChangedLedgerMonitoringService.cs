using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;

namespace SchoolAccount.CollectNotifications.Services;

public class StatusChangedLedgerMonitoringService(
    StatusChangedLedgerMonitoringServiceInstrumentation log,
    ILastRanService lastRanService,
    ILedgerStore ledgerStore,
    IThreadingService threadingService,
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

        await threadingService.Batch(
            whatToNotify,
            cancellationToken,
            async (notify, token) =>
            {
                token.ThrowIfCancellationRequested();
                log.NotifyingRecipient(notify.Recipient, notify.LaeStab);

                var result = await govNotifyService.SendMessage(
                    GovNotifyTemplates.CensusStatusChange,
                    notify.Recipient,
                    new Dictionary<string, dynamic>
                    {
                        { "status", notify.Status },
                        { "school_name", notify.School }
                    });

                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    log.NotificationRejected(result.Error);
                }

                return result.IsSuccess;
            });

        log.RunFinished((DateTime.UtcNow - runningAt).TotalMilliseconds);
    }
}
