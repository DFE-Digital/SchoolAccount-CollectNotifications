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
    public async Task<Result<DateTime>> GetTimestampAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
                           SELECT LastRun
                           FROM JobStatus
                           WHERE Name = @Name;
                           """;

        await using var conn = await factory.OpenAsync(cancellationToken);

        var lastRun = await conn.QuerySingleOrDefaultAsync<DateTime?>(
            new CommandDefinition(
                sql,
                new { Name = censusOptions.Value.JobName },
                cancellationToken: cancellationToken));

        // No row yet means we have never run. Everything in the ledger then counts as new, so the
        // row wants seeding at deploy rather than being left to default.
        return Result.Success(lastRun ?? (DateTime)SqlDateTime.MinValue);
    }

    public async Task<Result> SetTimestampAsync(DateTime timestamp, CancellationToken cancellationToken = default)
    {
        // One row per job, so update it if it is there and insert it if it isn't. A single nightly
        // writer, so there is no race worth using MERGE for.
        const string sql = """
                           UPDATE JobStatus
                           SET LastRun = @LastRun
                           WHERE Name = @Name;

                           IF @@ROWCOUNT = 0
                               INSERT INTO JobStatus (Name, LastRun)
                               VALUES (@Name, @LastRun);
                           """;

        await using var conn = await factory.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Name = censusOptions.Value.JobName, LastRun = timestamp },
                cancellationToken: cancellationToken));

        return Result.Success();
    }
}
