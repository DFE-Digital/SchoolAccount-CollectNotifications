using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Services.BlobStorage;

public class BlankedBlobStorageService : IBlobStorageService
{
    public async Task<Result<T?>> GetAsync<T>(string blobName, CancellationToken cancellationToken = default)
    {
        return Result.Success<T?>(default);
    }

    public async Task<Result> SaveAsync<T>(string blobName, T value, CancellationToken cancellationToken = default)
    {
        return Result.Success();
    }

    public async Task<Result<Stream?>> GetFileAsync(string blobName, CancellationToken cancellationToken = default)
    {
        return Result.Success<Stream?>(null);
    }
}