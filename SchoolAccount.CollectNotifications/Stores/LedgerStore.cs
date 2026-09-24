using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Stores;

public class LedgerStore(IDbConnectionFactory factory, IOptions<CensusOptions> censusOptions)
    : ILedgerStore
{
    public async Task<Result<List<CensusStatusChange>>> GetWhatHasChangedAsync(
        DateTime lastRunDate,
        CancellationToken cancellationToken = default
    )
    {
        // The rules are written as start status to end status, so they are applied to each
        // transition rather than to the net difference across the window. If we miss a run, a
        // return that reached Approved and then came off it again still produces both
        // notifications, because both rows are sitting in the ledger waiting to be read.
        //
        // LAG gives each row the status of the row before it, so there is no self join and no
        // rank. Recipients are joined after the window function, not inside it: joining them
        // first multiplies the rows and the window ends up counting rows times emails.
        const string sql = """
            WITH History AS (
                SELECT
                    SchoolName,
                    LAEStab,
                    ReturnStatusCode,
                    UpdatedAt,
                    Collection,
                    DCID,
                    LAG(ReturnStatusCode) OVER (
                        PARTITION BY LAEStab, Collection
                        ORDER BY UpdatedAt, Id
                    ) AS PreviousReturnStatusCode
                FROM CollectReturnStatus
                WHERE Collection = @Collection
            )
            SELECT
                h.SchoolName,
                h.LAEStab AS LaeStab,
                ru.Email,
                h.ReturnStatusCode,
                h.PreviousReturnStatusCode,
                h.UpdatedAt,
                h.Collection,
                h.DCID AS DcId
            FROM History h
                INNER JOIN RegisteredUsers ru
                    ON ru.LAEStab = h.LAEStab
            WHERE
                h.UpdatedAt >= @LastRunDate
                AND (
                    h.PreviousReturnStatusCode IS NULL
                    OR h.ReturnStatusCode <> h.PreviousReturnStatusCode
                )
                AND (
                    h.ReturnStatusCode IN @NotifiableStatuses
                    OR h.PreviousReturnStatusCode IN @NotifiableStatuses
                )
            ORDER BY h.LAEStab, h.UpdatedAt, ru.Email;
            """;

        try
        {
            await using var conn = await factory.OpenAsync(cancellationToken);

            var command = new CommandDefinition(
                sql,
                new
                {
                    censusOptions.Value.Collection,
                    LastRunDate = lastRunDate,
                    NotifiableStatuses = censusOptions.Value.AllowedStatuses,
                },
                cancellationToken: cancellationToken
            );

            var changes = await conn.QueryAsync<CensusStatusChange>(command);

            return Result.Success(changes.ToList());
        }
        catch (DbException exception)
        {
            return Result.Failure<List<CensusStatusChange>>(exception.Message);
        }
    }
}
