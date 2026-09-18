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
                        ) AS RnLast,
                        ROW_NUMBER() OVER (
                          PARTITION BY {nameof(CollectReturnStatus.LaeStab)}
                          ORDER BY {nameof(CollectReturnStatus.UpdatedAt)} ASC, {nameof(CollectReturnStatus.Id)} ASC
                        ) AS RnFirst
                    FROM {nameof(CollectReturnStatus)}
                    WHERE {nameof(CollectReturnStatus.LaeStab)} IN @LaeStabKeys
                    AND {nameof(CollectReturnStatus.UpdatedAt)} < @LastRunDate
                  )
                  SELECT 
                    l.*,
                    p.{nameof(CollectReturnStatus.ReturnStatusCode)} AS {nameof(ComparableCollectReturnStatus.PreviousReturnStatusCode)},
                    b.{nameof(CollectReturnStatus.ReturnStatusCode)} AS {nameof(ComparableCollectReturnStatus.InitialReturnStatusCode)}
                  FROM 
                    Latest l
                    LEFT JOIN Baseline p
                        ON p.{nameof(CollectReturnStatus.LaeStab)} = l.{nameof(CollectReturnStatus.LaeStab)} 
                        AND p.RnLast = 1
                    LEFT JOIN Baseline b 
                        ON b.{nameof(CollectReturnStatus.LaeStab)} = l.{nameof(CollectReturnStatus.LaeStab)} 
                        AND b.RnFirst = 1
                  WHERE 
                    l.Rn = 1
                    AND (
                        p.{nameof(CollectReturnStatus.LaeStab)} IS NULL 
                        OR l.{nameof(CollectReturnStatus.ReturnStatusCode)} <> p.{nameof(CollectReturnStatus.ReturnStatusCode)}
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
            .Where(x => HasValidStatus(x.PreviousReturnStatusCode, approvedStatuses)
                        || approvedStatuses.Contains(x.ReturnStatusCode)
                        || HasValidStatus(x.InitialReturnStatusCode, approvedStatuses));
    }

    private static bool HasValidStatus(ReturnStatusCodes? status, List<ReturnStatusCodes> approvedStatuses)
    {
        return status.HasValue && approvedStatuses.Contains(status.Value);
    }
}
