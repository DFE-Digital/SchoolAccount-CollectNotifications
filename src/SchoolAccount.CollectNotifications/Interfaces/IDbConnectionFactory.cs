using System.Data.Common;

namespace SchoolAccount.CollectNotifications.Interfaces;

public interface IDbConnectionFactory
{
    Task<DbConnection> OpenAsync(CancellationToken ct = default);
}
