using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Interfaces;

public interface ILastRanService
{
    /// <summary>Null when the job has never recorded a run.</summary>
    Task<Result<DateTime?>> GetTimestampAsync(CancellationToken cancellationToken = default);
    Task<Result> SetTimestampAsync(
        DateTime timestamp,
        CancellationToken cancellationToken = default
    );
}
