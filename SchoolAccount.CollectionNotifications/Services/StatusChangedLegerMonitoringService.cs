using SchoolAccount.CollectionNotifications.Interfaces;
using SchoolAccount.CollectionNotifications.Stores;

namespace SchoolAccount.CollectionNotifications.Services;

public class StatusChangedLegerMonitoringService(
    IEnrollmentStore enrollmentStore,
    LastRanService lastRanService,
    LedgerStore ledgerStore
)
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var recipients = await enrollmentStore.ListAsync(cancellationToken);
        var lastRan = await lastRanService.GetTimestamp(cancellationToken);
        var changes = await ledgerStore.GetWhatHasChangedAsync(
            lastRan, 
            recipients.Select(x => x.LAEStab).Distinct().ToList(), 
            cancellationToken);
        return;
    }
}