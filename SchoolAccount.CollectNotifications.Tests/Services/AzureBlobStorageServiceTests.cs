using System.Text;
using System.Text.Json;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services.BlobStorage;

namespace SchoolAccount.CollectNotifications.Tests.Services;

public class AzureBlobStorageServiceTests
{
    private readonly BlobServiceClient _serviceClient = Substitute.For<BlobServiceClient>();
    private readonly BlobContainerClient _containerClient = Substitute.For<BlobContainerClient>();
    private readonly BlobClient _blobClient = Substitute.For<BlobClient>();

    private readonly IOptions<AzureBlobStorageOptions> _options = Options.Create(new AzureBlobStorageOptions
    {
        ContainerName = "test-container"
    });

    private readonly AzureBlobStorageService _sut;

    public record TestModel(string Name, int Count);

    public AzureBlobStorageServiceTests()
    {
        _serviceClient.GetBlobContainerClient("test-container").Returns(_containerClient);
        _containerClient.GetBlobClient(Arg.Any<string>()).Returns(_blobClient);
        _sut = new AzureBlobStorageService(_serviceClient, _options, NullLogger<AzureBlobStorageService>.Instance);
    }

    [Fact]
    public async Task GetAsync_should_return_success_with_deserialized_content_when_blob_exists()
    {
        // Arrange
        var expectedModel = new TestModel("Alpha", 42);
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(expectedModel);
        var stream = new MemoryStream(jsonBytes);
        var downloadResult = BlobsModelFactory.BlobDownloadStreamingResult(content: stream);
        var response = Response.FromValue(downloadResult, Substitute.For<Response>());

        _blobClient.DownloadStreamingAsync(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _sut.GetAsync<TestModel>("my-blob.json");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Alpha");
        result.Value.Count.ShouldBe(42);
    }

    [Fact]
    public async Task GetAsync_should_return_success_with_null_when_blob_is_not_found_with_status_404()
    {
        // Arrange
        _blobClient.DownloadStreamingAsync(cancellationToken: Arg.Any<CancellationToken>())
            .Returns<Response<BlobDownloadStreamingResult>>(_ =>
                throw new RequestFailedException(404, "Blob not found"));

        // Act
        var result = await _sut.GetAsync<TestModel>("missing-blob.json");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_should_return_failure_when_deserialized_json_is_null()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("null"));
        var downloadResult = BlobsModelFactory.BlobDownloadStreamingResult(content: stream);
        var response = Response.FromValue(downloadResult, Substitute.For<Response>());

        _blobClient.DownloadStreamingAsync(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _sut.GetAsync<TestModel>("null-blob.json");

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task GetAsync_should_return_failure_when_unexpected_exception_is_thrown()
    {
        // Arrange
        _blobClient.DownloadStreamingAsync(cancellationToken: Arg.Any<CancellationToken>())
            .Returns<Response<BlobDownloadStreamingResult>>(_ =>
                throw new RequestFailedException(500, "Internal Server Error"));

        // Act
        var result = await _sut.GetAsync<TestModel>("error-blob.json");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("Internal Server Error");
    }

    [Fact]
    public async Task SaveAsync_should_return_success_when_upload_succeeds_with_valid_etag()
    {
        // Arrange
        var contentInfo = BlobsModelFactory.BlobContentInfo(new ETag("test-etag-value"), DateTimeOffset.UtcNow, null,
            null, null, null, 1);
        var response = Response.FromValue(contentInfo, Substitute.For<Response>());

        _blobClient.UploadAsync(
                Arg.Any<BinaryData>(),
                Arg.Any<BlobUploadOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _sut.SaveAsync("my-blob.json", new TestModel("Alpha", 1));

        // Arrange
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task SaveAsync_should_return_failure_when_upload_fails_due_to_request_failed_exception()
    {
        // Arrange
        _blobClient.UploadAsync(
                Arg.Any<BinaryData>(),
                Arg.Any<BlobUploadOptions>(),
                Arg.Any<CancellationToken>())
            .Returns<Response<BlobContentInfo>>(_ =>
                throw new RequestFailedException(403, "Forbidden", "AuthorizationPermissionMismatch", null));

        // Act
        var result = await _sut.SaveAsync("forbidden-blob.json", new TestModel("Beta", 2));

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("Upload failed: 403 AuthorizationPermissionMismatch");
    }

    [Fact]
    public async Task SaveAsync_should_return_failure_when_authentication_failed_exception_is_thrown()
    {
        // Arrange
        _blobClient.UploadAsync(
                Arg.Any<BinaryData>(),
                Arg.Any<BlobUploadOptions>(),
                Arg.Any<CancellationToken>())
            .Returns<Response<BlobContentInfo>>(_ => throw new AuthenticationFailedException("Token expired"));

        // Act
        var result = await _sut.SaveAsync("auth-fail-blob.json", new TestModel("Gamma", 3));

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Authentication failed");
    }

    [Fact]
    public async Task SaveAsync_should_return_failure_when_general_exception_is_thrown()
    {
        // Arrange
        _blobClient.UploadAsync(
                Arg.Any<BinaryData>(),
                Arg.Any<BlobUploadOptions>(),
                Arg.Any<CancellationToken>())
            .Returns<Response<BlobContentInfo>>(_ => throw new InvalidOperationException("Connection broken"));

        // Act
        var result = await _sut.SaveAsync("error-blob.json", new TestModel("Delta", 4));

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Upload failed: Connection broken");
    }
}
