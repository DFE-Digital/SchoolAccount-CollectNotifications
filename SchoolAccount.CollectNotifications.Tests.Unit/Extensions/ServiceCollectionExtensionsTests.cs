using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models.Databases;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services.BlobStorage;

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

    [Fact]
    public void When_adding_azure_blob_store_and_when_not_configured_should_register_blanked_blob_storage_service()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        services.AddAzureBlobStorage(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IBlobStorageService>();
        service.ShouldNotBeNull();
        service.ShouldBeOfType<BlankedBlobStorageService>();
    }

    [Fact]
    public void When_adding_azure_blob_store_and_connection_string_is_configured_should_register_azure_blob_service()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{AzureBlobStorageOptions.SectionName}:ConnectionString"] = "UseDevelopmentStorage=true",
                [$"{AzureBlobStorageOptions.SectionName}:ContainerName"] = "test-container"
            })
            .Build();

        // Act
        services.AddAzureBlobStorage(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IBlobStorageService>();
        service.ShouldNotBeNull();
        service.ShouldBeOfType<AzureBlobStorageService>();

        var client = provider.GetService<BlobServiceClient>();
        client.ShouldNotBeNull();
    }

    [Fact]
    public void When_adding_azure_blob_store_and_when_service_uri_is_configured_should_register_azure_blob_service()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{AzureBlobStorageOptions.SectionName}:ServiceUri"] = "https://testaccount.blob.core.windows.net",
                [$"{AzureBlobStorageOptions.SectionName}:ContainerName"] = "test-container"
            })
            .Build();

        // Act
        services.AddAzureBlobStorage(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IBlobStorageService>();
        service.ShouldNotBeNull();
        service.ShouldBeOfType<AzureBlobStorageService>();

        var client = provider.GetService<BlobServiceClient>();
        client.ShouldNotBeNull();
        client.Uri.AbsoluteUri.ShouldBe("https://testaccount.blob.core.windows.net/");
    }

    [Fact]
    public void When_creating_blob_client_from_service_provider_should_resolve_configured_client()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(new AzureBlobStorageOptions
        {
            ConnectionString = "UseDevelopmentStorage=true"
        }));
        var provider = services.BuildServiceProvider();

        // Act
        var client = ServiceCollectionExtensions.CreateBlobServiceClient(provider);

        // Assert
        client.ShouldNotBeNull();
    }

    [Fact]
    public void When_creating_blob_client_with_connection_string_should_create_client()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ConnectionString = "UseDevelopmentStorage=true"
        };

        // Act
        var client = ServiceCollectionExtensions.CreateBlobServiceClient(options);

        // Assert
        client.ShouldNotBeNull();
    }

    [Fact]
    public void When_creating_blob_client_with_service_uri_should_create_client()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ServiceUri = "https://testaccount.blob.core.windows.net"
        };

        // Act
        var client = ServiceCollectionExtensions.CreateBlobServiceClient(options);

        // Assert
        client.ShouldNotBeNull();
        client.Uri.AbsoluteUri.ShouldBe("https://testaccount.blob.core.windows.net/");
    }

    [Fact]
    public void When_creating_blob_client_with_neither_connection_string_nor_uri_should_throw_invalid_operation_exception()
    {
        // Arrange
        var options = new AzureBlobStorageOptions
        {
            ConnectionString = "",
            ServiceUri = ""
        };

        // Act & Assert
        var ex = Should.Throw<InvalidOperationException>(() => ServiceCollectionExtensions.CreateBlobServiceClient(options));
        ex.Message.ShouldContain("Set either AzureBlobStorage:ConnectionString or AzureBlobStorage:ServiceUri.");
    }
}
