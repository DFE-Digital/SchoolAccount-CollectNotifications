using Dapper;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Databases;

namespace SchoolAccount.CollectNotifications.Stores;

public class LedgerStore(
    IDbConnectionFactory<LedgerDatabase> factory
) : ILedgerStore
{
    public async Task<Result<List<CollectReturnStatus>>> GetWhatHasChangedAsync(DateTime lastRunDate, List<string> laeStabKeys,
        CancellationToken cancellationToken = default)
    {
        var sql = $"""
                  WITH Latest AS (
                    SELECT 
                        *,
                        ROW_NUMBER() OVER (
                            PARTITION BY {nameof(CollectReturnStatus.LaeStab)} 
                            ORDER BY {nameof(CollectReturnStatus.UpdatedAt)} DESC, {nameof(CollectReturnStatus.Id)} DESC
                        ) AS Rn
                    FROM {nameof(CollectReturnStatus)}
                    WHERE {nameof(CollectReturnStatus.LaeStab)} IN @LaeStabKeys
                    AND {nameof(CollectReturnStatus.UpdatedAt)} >= @LastRunDate
                  ),
                  Baseline AS (
                    SELECT 
                        {nameof(CollectReturnStatus.LaeStab)}, 
                        {nameof(CollectReturnStatus.ReturnStatusCode)},
                        ROW_NUMBER() OVER (
                            PARTITION BY {nameof(CollectReturnStatus.LaeStab)} 
                            ORDER BY {nameof(CollectReturnStatus.UpdatedAt)} DESC, {nameof(CollectReturnStatus.Id)} DESC
                        ) AS Rn
                    FROM {nameof(CollectReturnStatus)}
                    WHERE {nameof(CollectReturnStatus.LaeStab)} IN @LaeStabKeys
                    AND {nameof(CollectReturnStatus.UpdatedAt)} < @LastRunDate
                  )
                  SELECT 
                    l.*
                  FROM 
                    Latest l
                    LEFT JOIN Baseline b ON b.{nameof(CollectReturnStatus.LaeStab)} = l.{nameof(CollectReturnStatus.LaeStab)} AND b.Rn = 1
                  WHERE 
                    l.Rn = 1
                    AND (
                        b.{nameof(CollectReturnStatus.LaeStab)} IS NULL 
                        OR l.{nameof(CollectReturnStatus.ReturnStatusCode)} <> b.{nameof(CollectReturnStatus.ReturnStatusCode)}
                    )
                  ORDER BY l.{nameof(CollectReturnStatus.LaeStab)};
                  """;
        
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
