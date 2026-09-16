using Dapper;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Databases;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Stores;

public class LedgerStore(
    IDbConnectionFactory<LedgerDatabase> factory,
    IOptions<CensusOptions> censusOptions
) : ILedgerStore
{
    public async Task<Result<List<ComparableCollectReturnStatus>>> GetWhatHasChangedAsync(
        DateTime lastRunDate, 
        List<string> laeStabKeys,
        bool limitToApprovedStatuses = true,
        CancellationToken cancellationToken = default)
    {
        var sql = $"""
                  WITH Windowed AS (
                    SELECT 
                        *,
                        ROW_NUMBER() OVER (
                            PARTITION BY {nameof(CollectReturnStatus.LaeStab)} 
                            ORDER BY {nameof(CollectReturnStatus.UpdatedAt)} DESC, {nameof(CollectReturnStatus.Id)} DESC
                        ) AS RnLast,
                        ROW_NUMBER() OVER (
                            PARTITION BY {nameof(CollectReturnStatus.LaeStab)} 
                            ORDER BY {nameof(CollectReturnStatus.UpdatedAt)} ASC, {nameof(CollectReturnStatus.Id)} ASC
                        ) AS RnFirst
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
                    l.*,
                    f.{nameof(CollectReturnStatus.ReturnStatusCode)} AS {nameof(ComparableCollectReturnStatus.FirstReturnStatusCode)},
                    b.{nameof(CollectReturnStatus.ReturnStatusCode)} AS {nameof(ComparableCollectReturnStatus.BaselineReturnStatusCode)}
                  FROM 
                    Windowed l
                    INNER JOIN Windowed f 
                        ON f.{nameof(CollectReturnStatus.LaeStab)} = l.{nameof(CollectReturnStatus.LaeStab)} 
                        AND f.RnFirst = 1
                    LEFT JOIN Baseline b 
                        ON b.{nameof(CollectReturnStatus.LaeStab)} = l.{nameof(CollectReturnStatus.LaeStab)} 
                        AND b.Rn = 1
                  WHERE 
                    l.RnLast = 1
                    AND (
                        b.{nameof(CollectReturnStatus.LaeStab)} IS NULL 
                        OR l.{nameof(CollectReturnStatus.ReturnStatusCode)} <> b.{nameof(CollectReturnStatus.ReturnStatusCode)}
                    )
                  ORDER BY l.{nameof(CollectReturnStatus.LaeStab)};
                  """;
        
        await using var conn = await factory.OpenAsync(cancellationToken);
        var query = await conn
            .QueryAsync<ComparableCollectReturnStatus>(
                sql,
                new
                {
                    LastRunDate = lastRunDate,
                    LaeStabKeys = laeStabKeys
                });

        if (!limitToApprovedStatuses)
        {
            return Result.Success(query.ToList());
        }

        var filtered = query.RestrictToApprovedStatuses(censusOptions.Value.AllowedStatuses);
        return Result.Success(filtered.ToList());
    }
}

public static class LedgerStoreExtensions
{
    public static IEnumerable<ComparableCollectReturnStatus> RestrictToApprovedStatuses(
        this IEnumerable<ComparableCollectReturnStatus> collection,
        List<ReturnStatusCodes> approvedStatuses)
    {
        return collection
            .Where(x => HasValidStatus(x.FirstReturnStatusCode, approvedStatuses)
                        || approvedStatuses.Contains(x.ReturnStatusCode)
                        || HasValidStatus(x.BaselineReturnStatusCode, approvedStatuses));
    }

    private static bool HasValidStatus(ReturnStatusCodes? status, List<ReturnStatusCodes> approvedStatuses)
    {
        return status.HasValue && approvedStatuses.Contains(status.Value);
    }
}
