using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Interfaces;

public interface ILastRanService
{
    Task<Result<DateTime>> GetTimestampAsync(CancellationToken cancellationToken = default);
    Task<Result> SetTimestampAsync(
        DateTime timestamp,
        CancellationToken cancellationToken = default
    );
}
