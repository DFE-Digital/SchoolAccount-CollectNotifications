using Dapper;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Databases;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Stores;

public class LedgerStore(
    IDbConnectionFactory<LedgerDatabase> factory,
    IOptions<CensusOptions> censusOptions
) : ILedgerStore
{
    public async Task<Result<List<CensusStatusChange>>> GetWhatHasChangedAsync(
        DateTime lastRunDate,
        bool limitToApprovedStatuses = true,
        CancellationToken cancellationToken = default)
    {
        // Recipients are joined after the ranking, not before. Joining RegisteredUsers inside the
        // CTE multiplies each ledger row by the number of registered contacts, so rn 1 and rn 2
        // both end up being the same ledger row with different emails, their statuses compare
        // equal, and a school with more than one contact is silently never notified.
        var statusFilter = limitToApprovedStatuses
            ? """
                AND (
                    curr.ReturnStatusCode IN @NotifiableStatuses
                    OR prev.ReturnStatusCode IN @NotifiableStatuses
                )
              """
            : string.Empty;

        var sql = $"""
                   WITH Ranked AS (
                       SELECT
                           Id,
                           SchoolName,
                           LAEStab,
                           ReturnStatusCode,
                           UpdatedAt,
                           Collection,
                           DCID,
                           ROW_NUMBER() OVER (
                               PARTITION BY LAEStab, Collection
                               ORDER BY UpdatedAt DESC, Id DESC
                           ) AS Rn
                       FROM CollectReturnStatus
                       WHERE Collection = @Collection
                   )
                   SELECT
                       curr.SchoolName,
                       curr.LAEStab AS LaeStab,
                       ru.Email,
                       curr.ReturnStatusCode,
                       prev.ReturnStatusCode AS PreviousReturnStatusCode,
                       curr.UpdatedAt,
                       curr.Collection,
                       curr.DCID AS DcId
                   FROM Ranked curr
                       LEFT JOIN Ranked prev
                           ON prev.LAEStab = curr.LAEStab
                           AND prev.Collection = curr.Collection
                           AND prev.Rn = 2
                       INNER JOIN RegisteredUsers ru
                           ON ru.LAEStab = curr.LAEStab
                   WHERE
                       curr.Rn = 1
                       AND curr.UpdatedAt >= @LastRunDate
                       AND (
                           prev.ReturnStatusCode IS NULL
                           OR curr.ReturnStatusCode <> prev.ReturnStatusCode
                       )
                       {statusFilter}
                   ORDER BY curr.LAEStab, ru.Email;
                   """;

        await using var conn = await factory.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new
            {
                Collection = censusOptions.Value.Collection,
                LastRunDate = lastRunDate,
                NotifiableStatuses = censusOptions.Value.AllowedStatuses
            },
            cancellationToken: cancellationToken);

        var changes = await conn.QueryAsync<CensusStatusChange>(command);

        return Result.Success(changes.ToList());
    }
}
