using System.Data.SqlTypes;
using SchoolAccount.CollectionNotifications.Models;

namespace SchoolAccount.CollectionNotifications.Services;

public class LastRanService
{
    public async Task<Result<DateTime>> GetTimestamp(CancellationToken cancellationToken)
    {
        return Result.Success((DateTime)SqlDateTime.MinValue);
    }

    public async Task<Result> SetTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}