using System.Data.Common;
using Microsoft.Data.SqlClient;
using SchoolAccount.CollectionNotifications.Interfaces;

namespace SchoolAccount.CollectionNotifications.Models;

public sealed class DbConnectionFactory<TDb>(string connectionString) : IDbConnectionFactory<TDb>, IDisposable, IAsyncDisposable
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