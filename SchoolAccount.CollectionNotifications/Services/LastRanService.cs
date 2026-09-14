using System.Data.SqlTypes;

namespace SchoolAccount.CollectionNotifications.Services;

public class LastRanService
{
    public async Task<DateTime> GetTimestamp(CancellationToken cancellationToken)
    {
        return (DateTime)SqlDateTime.MinValue;
    }

    public async Task SetTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}