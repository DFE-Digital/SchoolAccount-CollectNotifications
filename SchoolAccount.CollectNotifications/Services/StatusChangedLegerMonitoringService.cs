using Microsoft.Extensions.Logging;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Stores;

namespace SchoolAccount.CollectNotifications.Services;

public class StatusChangedLegerMonitoringService(
    ILogger<StatusChangedLegerMonitoringService> logger,
    IEnrollmentStore enrollmentStore,
    LastRanService lastRanService,
    LedgerStore ledgerStore,
    ThreadingService threadingService,
    GovNotifyService govNotifyService
)
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var runningAt = DateTime.UtcNow;

        logger.LogInformation("Running {class} at {runningAt}", nameof(StatusChangedLegerMonitoringService), runningAt);

        var recipients = await enrollmentStore.ListAsync(cancellationToken);

        if (recipients.IsFailure)
        {
            logger.LogWarning("Retrieving recipients list failed: {error}", recipients.Error);
            return;
        }

        logger.LogInformation("Received {count} of recipients", recipients.Value.Count);

        var lastRan = await lastRanService.GetTimestamp(cancellationToken);

        if (lastRan.IsFailure)
        {
            logger.LogWarning("Retrieving last ran date failed: {error}", lastRan.Error);
            return;
        }

        logger.LogInformation("Service last run {lastRan}", lastRan.Value);

        var changes = await ledgerStore.GetWhatHasChangedAsync(
            lastRan.Value,
            recipients.Value.Select(x => x.LaeStab).Distinct().ToList(),
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
            var toNotify = recipients.Value.Where(x => x.LaeStab == change.LaeStab).ToList();
            logger.LogInformation("For {laeStab} {count} will be notified", change.LaeStab, toNotify.Count);

            foreach (var notify in toNotify)
            {
                logger.LogInformation("Adding notification record for {recipient} at {laeStab}", notify.Email, change.LaeStab);
                
                whatToNotify.Add(
                    new Notification(
                        change.LaeStab,
                        notify.Email,
                        change.ReturnStatusCode.ToString(),
                        change.SchoolName));
            }
        }

        await lastRanService.SetTimestamp(runningAt, cancellationToken);

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
                
                if (result.IsFailure)
                {
                    logger.LogWarning("Sending email to {recipient} failed: {message}", notify.Recipient, result.Error);
                    return false;
                }

                return true;
            });

        var completedOn = DateTime.UtcNow;
        logger.LogInformation("Completed at {completedOn} took {time}ms", completedOn,
            (completedOn - runningAt).TotalMilliseconds);
    }
}