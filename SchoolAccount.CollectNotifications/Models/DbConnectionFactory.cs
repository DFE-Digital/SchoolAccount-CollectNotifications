using System.Data.Common;
using Microsoft.Data.SqlClient;
using SchoolAccount.CollectNotifications.Interfaces;

namespace SchoolAccount.CollectNotifications.Models;

public sealed class DbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public async Task<DbConnection> OpenAsync(CancellationToken ct = default)
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        return connection;
    }
}
