using Dapper;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Databases;

namespace SchoolAccount.CollectNotifications.Stores;

public class LedgerStore(
    IDbConnectionFactory<LedgerDatabase> factory
)
{
    public async Task<Result<List<CollectReturnStatus>>> GetWhatHasChangedAsync(DateTime lastRunDate, List<string> laeStabKeys,
        CancellationToken cancellationToken)
    {
        var sql = @$"
                  SELECT *
                  FROM {nameof(CollectReturnStatus)}
                  WHERE {nameof(CollectReturnStatus.UpdatedAt)} >= @LastRunDate
                        AND {nameof(CollectReturnStatus.LaeStab)} IN @LaeStabKeys
                  ";
        
        await using var conn = await factory.OpenAsync(cancellationToken);
        var query = await conn
            .QueryAsync<CollectReturnStatus>(
                sql,
                new
                {
                    LastRunDate = lastRunDate,
                    LaeStabKeys = laeStabKeys
                });
        
        return Result.Success(query.ToList());
    }
}