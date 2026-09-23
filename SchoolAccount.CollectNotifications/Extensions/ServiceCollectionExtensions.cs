using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
                               ?? throw new ArgumentException(
                                   $"Connection string for {connectionStringName} was not found.");

        return services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
    }
}
