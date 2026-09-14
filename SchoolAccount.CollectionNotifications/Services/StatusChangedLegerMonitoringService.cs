using SchoolAccount.CollectionNotifications.Interfaces;

namespace SchoolAccount.CollectionNotifications.Services;

public class StatusChangedLegerMonitoringService(
    IEnrollmentStore enrollmentStore
)
{
    public async Task InvokeAsync(CancellationToken cancellationToken = default)
    {
        var recipients = await enrollmentStore.ListAsync(cancellationToken);
        return;
    }
}