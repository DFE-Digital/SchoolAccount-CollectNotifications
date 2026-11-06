using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.UnitTests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void Adding_a_database_should_resolve_db_connection_factory_using_census_options()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(
            Options.Create(
                new CensusOptions { ConnectionString = "Server=sql.example.com;Database=Ledger;" }
            )
        );

        // Act
        services.AddDatabase();
        var provider = services.BuildServiceProvider();

        // Assert
        var factory = provider.GetService<IDbConnectionFactory>();
        factory.ShouldNotBeNull();
        factory.ShouldBeOfType<DbConnectionFactory>();
    }

    [Fact]
    public void Resolving_database_factory_without_census_options_should_throw()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDatabase();
        var provider = services.BuildServiceProvider();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            provider.GetRequiredService<IDbConnectionFactory>()
        );
    }
}
