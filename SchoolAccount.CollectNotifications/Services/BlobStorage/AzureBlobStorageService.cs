using System.Text.Json;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Services.BlobStorage;

public class AzureBlobStorageService(
    BlobServiceClient serviceClient, 
    IOptions<AzureBlobStorageOptions> options,
    ILogger<AzureBlobStorageService> logger
) : IBlobStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly BlobContainerClient _container = serviceClient.GetBlobContainerClient(options.Value.ContainerName);

    public async Task<Result<T?>> GetAsync<T>(string blobName, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(blobName);

        try
        {
            var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
            await using var stream = response.Value.Content;
            var content = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
            
            return content is not null 
                ? Result.Success<T?>(content) 
                :  Result.Failure<T?>();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return Result.Success<T?>(default);
        }
        catch (Exception ex)
        {
            return Result.Failure<T?>(ex.Message);
        }
    }

    public async Task<Result> SaveAsync<T>(string blobName, T value, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(blobName);
        var data = BinaryData.FromObjectAsJson(value, JsonOptions);

        try
        {
            var outcome = await blob.UploadAsync(
                data,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "application/json"
                    }
                },
                cancellationToken);

            return outcome.HasValue && !string.IsNullOrEmpty(outcome.Value.ETag.ToString())
                ? Result.Success()
                : Result.Failure("Blob not defined a ETag");
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Azure Blob Storage: Upload failed: {Status} {ErrorCode}", ex.Status, ex.ErrorCode);
            return Result.Failure($"Upload failed: {ex.Status} {ex.ErrorCode}");
        }
        catch (AuthenticationFailedException ex)
        {
            logger.LogError(ex, "Azure Blob Storage: Couldn't authenticate to Azure");
            return Result.Failure("Authentication failed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure Blob Storage: Upload failed");
            return Result.Failure($"Upload failed: {ex.Message}");
        }
    }

    public async Task<Result<Stream?>> GetFileAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(blobName);

        try
        {
            var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
            return Result.Success<Stream?>(response.Value.Content);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return Result.Success<Stream?>(null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure Blob Storage: Failed to download blob {BlobName}", blobName);
            return Result.Failure<Stream?>(ex.Message);
        }
    }
}