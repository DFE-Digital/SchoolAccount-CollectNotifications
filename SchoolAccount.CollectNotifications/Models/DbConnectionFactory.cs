using System.Data.Common;
using Microsoft.Data.SqlClient;
using SchoolAccount.CollectNotifications.Interfaces;

namespace SchoolAccount.CollectNotifications.Models;

public sealed class DbConnectionFactory(string connectionString) : IDbConnectionFactory, IDisposable, IAsyncDisposable
{
    private SqlConnection? _connection;
    
    public async Task<DbConnection> OpenAsync(CancellationToken ct = default)
    {
        _connection = new SqlConnection(connectionString);
        await _connection.OpenAsync(ct);
        return _connection;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}
