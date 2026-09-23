using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using SchoolAccount.CollectNotifications.Models.Dtos;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Helpers;

public static class TestDatabaseHelper
{
    public const string DefaultConnectionString =
        "Server=localhost;Database=CollectStateLedger;User Id=sa;Password=MyStrongPassword123!;TrustServerCertificate=true";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("ConnectionStrings:LedgerDatabase")
        ?? DefaultConnectionString;

    public static async Task<DbConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default
    )
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public static async Task InsertReturnStatusesAsync(
        IEnumerable<CollectReturnStatus> records,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = $"""
            INSERT INTO {nameof(CollectReturnStatus)} (
                {nameof(CollectReturnStatus.SchoolName)},
                {nameof(CollectReturnStatus.LaeStab)},
                {nameof(CollectReturnStatus.ReturnStatusCode)},
                {nameof(CollectReturnStatus.Errors)},
                {nameof(CollectReturnStatus.Queries)},
                {nameof(CollectReturnStatus.OkdErrorsQueries)},
                {nameof(CollectReturnStatus.Hash)},
                {nameof(CollectReturnStatus.UpdatedAt)},
                {nameof(CollectReturnStatus.DcId)},
                {nameof(CollectReturnStatus.Collection)},
                {nameof(CollectReturnStatus.DataReturnId)}
            ) VALUES (
                @{nameof(CollectReturnStatus.SchoolName)},
                @{nameof(CollectReturnStatus.LaeStab)},
                @{nameof(CollectReturnStatus.ReturnStatusCode)},
                @{nameof(CollectReturnStatus.Errors)},
                @{nameof(CollectReturnStatus.Queries)},
                @{nameof(CollectReturnStatus.OkdErrorsQueries)},
                @{nameof(CollectReturnStatus.Hash)},
                @{nameof(CollectReturnStatus.UpdatedAt)},
                @{nameof(CollectReturnStatus.DcId)},
                @{nameof(CollectReturnStatus.Collection)},
                @{nameof(CollectReturnStatus.DataReturnId)}
            );
            """;

        await using var conn = await OpenConnectionAsync(cancellationToken);
        await conn.ExecuteAsync(sql, records);
    }

    public static async Task DeleteReturnStatusesByLaeStabAsync(
        IEnumerable<string> laeStabKeys,
        CancellationToken cancellationToken = default
    )
    {
        var keys = laeStabKeys.ToList();
        if (keys.Count == 0)
        {
            return;
        }

        const string sql = $"""
            DELETE FROM {nameof(CollectReturnStatus)}
            WHERE {nameof(CollectReturnStatus.LaeStab)} IN @Keys;
            """;

        await using var conn = await OpenConnectionAsync(cancellationToken);
        await conn.ExecuteAsync(sql, new { Keys = keys });
    }

    public static async Task InsertRegisteredUsersAsync(
        string laeStab,
        IEnumerable<string> emails,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
            INSERT INTO RegisteredUsers (LAEStab, Email)
            VALUES (@LaeStab, @Email);
            """;

        var rows = emails.Select(email => new { LaeStab = laeStab, Email = email }).ToList();

        await using var conn = await OpenConnectionAsync(cancellationToken);
        await conn.ExecuteAsync(sql, rows);
    }

    public static async Task DeleteRegisteredUsersByLaeStabAsync(
        IEnumerable<string> laeStabKeys,
        CancellationToken cancellationToken = default
    )
    {
        var keys = laeStabKeys.ToList();
        if (keys.Count == 0)
        {
            return;
        }

        const string sql = """
            DELETE FROM RegisteredUsers
            WHERE LAEStab IN @Keys;
            """;

        await using var conn = await OpenConnectionAsync(cancellationToken);
        await conn.ExecuteAsync(sql, new { Keys = keys });
    }
}
