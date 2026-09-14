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
    public async Task<Result> InvokeAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Running {class}", nameof(StatusChangedLegerMonitoringService));
        
        var recipients = await enrollmentStore.ListAsync(cancellationToken);
        logger.LogInformation("Received {count} of recipients", recipients.Value.Count);
        
        var lastRan = await lastRanService.GetTimestamp(cancellationToken);
        logger.LogInformation("Service last run {lastRan}", lastRan.Value);
        
        var changes = await ledgerStore.GetWhatHasChangedAsync(
            lastRan.Value, 
            recipients.Value.Select(x => x.LAEStab).Distinct().ToList(), 
            cancellationToken);
        logger.LogInformation("Found {count} of changes", changes.Value.Count);
        
        return Result.Success();
    }
}