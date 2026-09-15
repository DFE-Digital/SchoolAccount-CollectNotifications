using System.Data.SqlTypes;
using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Services;

public class LastRanService
{
    public async Task<Result<DateTime>> GetTimestamp(CancellationToken cancellationToken)
    {
        return Result.Success((DateTime)SqlDateTime.MinValue);
    }

    public async Task<Result> SetTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    {
        // Todo: implement a source
        return Result.Success();
    }
}