using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models.Databases;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void Adding_a_database_with_connectionStringFactory_should_register_db_connection_factory()
    {
        // Arrange
        var services = new ServiceCollection();
        const string expectedConnectionString = "Server=my-server;Database=testdb;";

        // Act
        services.AddDatabase<LedgerDatabase>(() => expectedConnectionString);
        var provider = services.BuildServiceProvider();

        // Assert
        var factory = provider.GetService<IDbConnectionFactory<LedgerDatabase>>();
        factory.ShouldNotBeNull();
    }

    [Fact]
    public void Adding_a_database_with_configuration_should_resolve_connection_string_by_type_name()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:LedgerDatabase"] = "Server=sql.example.com;Database=Ledger;"
            })
            .Build();

        // Act
        services.AddDatabase<LedgerDatabase>(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var factory = provider.GetService<IDbConnectionFactory<LedgerDatabase>>();
        factory.ShouldNotBeNull();
    }

    [Fact]
    public void Adding_a_database_with_missing_connection_string_in_configuration_should_throw_on_registration()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => services.AddDatabase<LedgerDatabase>(configuration));
        ex.Message.ShouldContain("Connection string for LedgerDatabase was not found.");
    }

}
