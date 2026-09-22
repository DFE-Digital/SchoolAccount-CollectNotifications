using Microsoft.Extensions.Logging;
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;

namespace SchoolAccount.CollectNotifications.Services;

public class StatusChangedLedgerMonitoringService(
    ILogger<StatusChangedLedgerMonitoringService> logger,
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

        var lastRan = await lastRanService.GetTimestampAsync(cancellationToken);

        if (lastRan.IsFailure)
        {
            logger.LogWarning("Retrieving last ran date failed: {error}", lastRan.Error);
            return;
        }

        logger.LogInformation("Service last run {lastRan}", lastRan.Value);

        var changes = await ledgerStore.GetWhatHasChangedAsync(lastRan.Value, true, cancellationToken);

        if (changes.IsFailure)
        {
            logger.LogWarning("Retrieving what had changed failed: {error}", changes.Error);
            return;
        }

        logger.LogInformation("Found {count} of changes", changes.Value.Count);

        // The query already pairs each change with its registered recipients, so a school with two
        // registered contacts arrives here as two changes.
        var whatToNotify = changes.Value
            .Select(change => new Notification(
                change.LaeStab,
                change.Email,
                change.ReturnStatusCode.GetHumanName(),
                change.SchoolName))
            .ToList();

        logger.LogInformation("Sending {count} of notifications", whatToNotify.Count);

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
