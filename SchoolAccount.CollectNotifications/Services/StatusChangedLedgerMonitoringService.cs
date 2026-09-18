using Microsoft.Extensions.Logging;
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;

namespace SchoolAccount.CollectNotifications.Services;

public class StatusChangedLedgerMonitoringService(
    ILogger<StatusChangedLedgerMonitoringService> logger,
    IEnrollmentStore enrollmentStore,
    ILastRanService lastRanService,
    ILedgerStore ledgerStore,
    IThreadingService threadingService,
    IGovNotifyService govNotifyService
)
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var runningAt = DateTime.UtcNow;

        logger.LogInformation("Running {class} at {runningAt}", nameof(StatusChangedLedgerMonitoringService), runningAt);

        var recipients = await enrollmentStore.ListAsync(cancellationToken);

        if (recipients.IsFailure)
        {
            logger.LogWarning("Retrieving recipients list failed: {error}", recipients.Error);
            return;
        }

        logger.LogInformation("Received {count} of recipients", recipients.Value.Count);

        var lastRan = await lastRanService.GetTimestampAsync(cancellationToken);

        if (lastRan.IsFailure)
        {
            logger.LogWarning("Retrieving last ran date failed: {error}", lastRan.Error);
            return;
        }

        logger.LogInformation("Service last run {lastRan}", lastRan.Value);

        var changes = await ledgerStore.GetWhatHasChangedAsync(
            lastRan.Value,
            recipients.Value.Where(x => x.LaeStab != null).Select(x => x.LaeStab!).Distinct().ToList(),
            true,
            cancellationToken);

        if (changes.IsFailure)
        {
            logger.LogWarning("Retrieving what had changed failed: {error}", changes.Error);
            return;
        }

        logger.LogInformation("Found {count} of changes", changes.Value.Count);

        var whatToNotify = new List<Notification>();
        foreach (var change in changes.Value)
        {
            var toNotify = recipients.Value.Where(x => x.LaeStab == change.LaeStab && !string.IsNullOrWhiteSpace(x.Email)).ToList();
            logger.LogInformation("For {laeStab} {count} will be notified", change.LaeStab, toNotify.Count);

            foreach (var notify in toNotify)
            {
                logger.LogInformation("Adding notification record for {recipient} at {laeStab}", notify.Email, change.LaeStab);
                
                whatToNotify.Add(
                    new Notification(
                        change.LaeStab,
                        notify.Email!,
                        change.ReturnStatusCode.GetHumanName(),
                        change.SchoolName));
            }
        }

        var timestampUpdate = await lastRanService.SetTimestampAsync(runningAt, cancellationToken);

        if (timestampUpdate.IsFailure)
        {
            logger.LogWarning("Updating last ran date failed: {error}", timestampUpdate.Error);
            return;
        }

        await threadingService.Batch(
            whatToNotify, 
            cancellationToken,
            async (notify, token) =>
            {
                token.ThrowIfCancellationRequested();
                logger.LogInformation("Sending email to {recipient} for {laeStab}", notify.Recipient, notify.LaeStab);
                
                var result = await govNotifyService.SendMessage(
                    GovNotifyTemplates.CensusStatusChange,
                    notify.Recipient,
                    new Dictionary<string, dynamic>
                    {
                        { "status", notify.Status }, // todo: actually make it a human equivalent 
                        { "school_name", notify.School }
                    });

                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    logger.LogWarning("Sending email message: {error}", result.Error);
                }

                return result.IsSuccess;
            });

        var completedOn = DateTime.UtcNow;
        logger.LogInformation("Completed at {completedOn} took {time}ms", completedOn,
            (completedOn - runningAt).TotalMilliseconds);
    }
}
