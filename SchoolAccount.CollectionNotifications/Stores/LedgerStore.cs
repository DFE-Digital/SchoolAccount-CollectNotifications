using Dapper;
using SchoolAccount.CollectionNotifications.Interfaces;
using SchoolAccount.CollectionNotifications.Models;
using SchoolAccount.CollectionNotifications.Models.Databases;

namespace SchoolAccount.CollectionNotifications.Stores;

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