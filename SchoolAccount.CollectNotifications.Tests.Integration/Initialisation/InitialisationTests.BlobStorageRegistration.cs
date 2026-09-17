using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services.BlobStorage;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Initialisation;

public partial class InitialisationTests
{
    [Fact]
    public void ConfigureService_should_register_azure_blob_service_and_bind_options_when_connection_string_is_configured()
    {
        const string testLocalBlobConnectionString = "UseDevelopmentStorage=true";
        const string testLocalBlobContainer = "test-container";
        
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            ["AzureBlobStorage:ConnectionString"] = testLocalBlobConnectionString,
            ["AzureBlobStorage:ContainerName"] = testLocalBlobContainer
        });

        // Act
        using var scope = host.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var blobService = sp.GetRequiredService<IBlobStorageService>();
        var blobOptions = sp.GetRequiredService<IOptions<AzureBlobStorageOptions>>().Value;
        
        // Assert
        blobService.ShouldBeOfType<AzureBlobStorageService>();

        blobOptions.ShouldSatisfyAllConditions(
            x => x.ConnectionString.ShouldBe(testLocalBlobConnectionString),
            x => x.ServiceUri.ShouldBeNullOrEmpty(),
            x => x.ContainerName.ShouldBe(testLocalBlobContainer)
        );
    }

    [Fact]
    public void ConfigureService_should_register_azure_blob_service_when_service_uri_is_configured()
    {
        const string testBlobServiceUri = "https://myaccount.blob.core.windows.net";
        const string testLocalBlobContainer = "test-container";
        
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            ["AzureBlobStorage:ServiceUri"] = testBlobServiceUri,
            ["AzureBlobStorage:ContainerName"] = testLocalBlobContainer
        });

        // Act
        using var scope = host.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var blobService = sp.GetRequiredService<IBlobStorageService>();
        var blobOptions = sp.GetRequiredService<IOptions<AzureBlobStorageOptions>>().Value;
        
        // Assert
        blobService.ShouldBeOfType<AzureBlobStorageService>();

        blobOptions.ShouldSatisfyAllConditions(
            x => x.ConnectionString.ShouldBeNullOrEmpty(),
            x => x.ServiceUri.ShouldBe(testBlobServiceUri),
            x => x.ContainerName.ShouldBe(testLocalBlobContainer)
        );
    }

    [Fact]
    public void ConfigureService_should_fallback_to_blanked_blob_storage_service_when_azure_blob_storage_is_not_configured()
    {
        // Arrange
        using var host = CreateHost([]);

        // Act
        using var scope = host.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var blobService = sp.GetRequiredService<IBlobStorageService>();
        var blobOptions = sp.GetRequiredService<IOptions<AzureBlobStorageOptions>>().Value;
        
        // Assert
        blobService.ShouldBeOfType<BlankedBlobStorageService>();
        blobOptions.IsConfigured.ShouldBeFalse();
    }
}
