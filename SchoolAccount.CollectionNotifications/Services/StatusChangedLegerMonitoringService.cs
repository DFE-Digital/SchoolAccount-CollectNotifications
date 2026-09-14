using Microsoft.Extensions.Logging;
using SchoolAccount.CollectionNotifications.Interfaces;
using SchoolAccount.CollectionNotifications.Models;
using SchoolAccount.CollectionNotifications.Stores;

namespace SchoolAccount.CollectionNotifications.Services;

public class StatusChangedLegerMonitoringService(
    ILogger<StatusChangedLegerMonitoringService> logger,
    IEnrollmentStore enrollmentStore,
    LastRanService lastRanService,
    LedgerStore ledgerStore,
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
            recipients.Value.Select(x => x.LAEStab).Distinct().ToList(),
            cancellationToken);

        if (changes.IsFailure)
        {
            logger.LogWarning("Retrieving what had changed failed: {error}", changes.Error);
            return;
        }
        
        logger.LogInformation("Found {count} of changes", changes.Value.Count);

        foreach (var change in changes.Value)
        {
            var toNotify = recipients.Value.Where(x => x.LAEStab == change.LaeStab).ToList();
            logger.LogInformation("For {laeStab} {count} will be notified", change.LaeStab, toNotify.Count);
            
            foreach (var notify in toNotify)
            {
                logger.LogInformation("Sending email to {recipient} for {laeStab}", notify.Email, change.LaeStab);
                
                var result = await govNotifyService.SendMessage(
                    GovNotifyTemplates.CensusStatusChange,
                    notify.Email,
                    new Dictionary<string, dynamic>
                    {
                        { "status", change.ReturnStatusCode }, // todo: actually make it a human equivalent 
                        { "school_name", change.SchoolName }
                    });
                
                if (result.IsFailure)
                {
                    logger.LogWarning("Sending email to {recipient} failed: {message}", notify.Email, result.Error);
                }
            }
        }

        await lastRanService.SetTimestamp(runningAt, cancellationToken);

        var completedOn = DateTime.UtcNow;
        logger.LogInformation("Completed at {completedOn} took {time}ms", completedOn, (completedOn - runningAt).TotalMilliseconds);
    }
}