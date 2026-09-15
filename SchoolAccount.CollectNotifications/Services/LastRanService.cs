using System.Data.SqlTypes;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Services;

public class LastRanService(
    IBlobStorageService blobStorageService
)
{
    private const string BlobName = "schoolaccount/collect/lastran.json";

    public record LastRanBlobObject(double RunDate);

    public async Task<Result<DateTime>> GetTimestampAsync(CancellationToken cancellationToken)
    {
        var blob = await blobStorageService.GetAsync<LastRanBlobObject>(BlobName, cancellationToken);

        if (blob.IsFailure)
        {
            return Result.Failure<DateTime>(blob.Error);
        }

        var runDate = blob.Value is not null 
            ? DateTime.FromOADate(blob.Value.RunDate) 
            : (DateTime)SqlDateTime.MinValue;
        
        return Result.Success(runDate);
    }

    public async Task<Result> SetTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var blob = new LastRanBlobObject(timestamp.ToOADate());
        return await blobStorageService.SaveAsync(BlobName, blob, cancellationToken);
    }
}