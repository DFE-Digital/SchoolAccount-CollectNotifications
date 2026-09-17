using System.Data.Common;

namespace SchoolAccount.CollectNotifications.Interfaces;

public interface IDbConnectionFactory<TDb>
{
    Task<DbConnection> OpenAsync(CancellationToken ct = default);
}