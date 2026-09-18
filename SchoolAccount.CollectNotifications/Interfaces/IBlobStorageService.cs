using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Interfaces;

public interface IBlobStorageService
{
    Task<Result<T?>> GetAsync<T>(string blobName, CancellationToken cancellationToken = default);
    Task<Result> SaveAsync<T>(string blobName, T value, CancellationToken cancellationToken = default);
    
    Task<Result<Stream?>> GetFileAsync(string blobName, CancellationToken cancellationToken = default);
}