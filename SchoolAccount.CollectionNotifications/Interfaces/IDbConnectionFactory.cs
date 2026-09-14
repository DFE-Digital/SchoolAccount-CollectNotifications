using System.Data.Common;

namespace SchoolAccount.CollectionNotifications.Interfaces;

public interface IDbConnectionFactory<TDb>
{
    Task<DbConnection> OpenAsync(CancellationToken ct = default);
}