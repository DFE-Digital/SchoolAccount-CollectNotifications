using System.Data.Common;
using System.Data.SqlTypes;
using Dapper;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Databases;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Services;

/// <summary>
/// Tracks when this job last completed, in the ledger database's JobStatus table.
/// </summary>
public class LastRanService(
    IDbConnectionFactory<LedgerDatabase> factory,
    IOptions<CensusOptions> censusOptions
) : ILastRanService
{
    /// <summary>
    /// What we report when the job has no row yet. Everything in the ledger is after this, so a
    /// run starting from here treats the whole history as new.
    /// </summary>
    public static readonly DateTime NeverRun = (DateTime)SqlDateTime.MinValue;

    public async Task<Result<DateTime>> GetTimestampAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
                           SELECT LastRun
                           FROM JobStatus
                           WHERE Name = @Name;
                           """;

        try
        {
            await using var conn = await factory.OpenAsync(cancellationToken);

            var lastRun = await conn.QuerySingleOrDefaultAsync<DateTime?>(
                new CommandDefinition(
                    sql,
                    new { Name = censusOptions.Value.JobName },
                    cancellationToken: cancellationToken));

            return Result.Success(lastRun ?? NeverRun);
        }
        catch (DbException exception)
        {
            return Result.Failure<DateTime>(exception.Message);
        }
    }

    public async Task<Result> SetTimestampAsync(DateTime timestamp, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           UPDATE JobStatus
                           SET LastRun = @LastRun
                           WHERE Name = @Name;

                           IF @@ROWCOUNT = 0
                               INSERT INTO JobStatus (Name, LastRun)
                               VALUES (@Name, @LastRun);
                           """;

        try
        {
            await using var conn = await factory.OpenAsync(cancellationToken);

            await conn.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new { Name = censusOptions.Value.JobName, LastRun = timestamp },
                    cancellationToken: cancellationToken));

            return Result.Success();
        }
        catch (DbException exception)
        {
            return Result.Failure(exception.Message);
        }
    }
}
