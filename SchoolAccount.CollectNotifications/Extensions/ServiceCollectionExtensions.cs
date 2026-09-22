using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase<TDb>(this IServiceCollection services, Func<string> connectionStringFactory)
    {
        var connectionString = connectionStringFactory();
        return services.AddSingleton<IDbConnectionFactory<TDb>>(_ => new DbConnectionFactory<TDb>(connectionString));
    }

    public static IServiceCollection AddDatabase<TDb>(this IServiceCollection services, IConfiguration configuration)
    {
        var identifier = typeof(TDb).Name;
        var factory = () => configuration.GetConnectionString(identifier)
                            ?? throw new ArgumentException($"Connection string for {identifier} was not found.");
        return services.AddDatabase<TDb>(factory);
    }
}
