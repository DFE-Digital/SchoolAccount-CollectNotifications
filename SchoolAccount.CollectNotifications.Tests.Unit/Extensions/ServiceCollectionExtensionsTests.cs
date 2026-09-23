using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Interfaces;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void Adding_a_database_should_resolve_the_named_connection_string()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:LedgerDatabase"] = "Server=sql.example.com;Database=Ledger;",
            })
            .Build();

        // Act
        services.AddDatabase(configuration, "LedgerDatabase");
        var provider = services.BuildServiceProvider();

        // Assert
        var factory = provider.GetService<IDbConnectionFactory>();
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
        var ex = Should.Throw<ArgumentException>(() => services.AddDatabase(configuration, "LedgerDatabase"));
        ex.Message.ShouldContain("Connection string for LedgerDatabase was not found.");
    }

}
