using System.Data.SqlTypes;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Services;

public class LastRanService(
    IBlobStorageService blobStorageService,
    IOptions<CensusOptions> censusOptions
) : ILastRanService
{
    public record LastRanBlobObject(double RunDate);

    public async Task<Result<DateTime>> GetTimestampAsync(CancellationToken cancellationToken = default)
    {
        var blob = await blobStorageService.GetAsync<LastRanBlobObject>(censusOptions.Value.LastRunBlobName,
            cancellationToken);

        if (blob.IsFailure)
        {
            return Result.Failure<DateTime>(blob.Error);
        }

        var runDate = blob.Value is not null
            ? DateTime.FromOADate(blob.Value.RunDate)
            : (DateTime)SqlDateTime.MinValue;

        return Result.Success(runDate);
    }

    public async Task<Result> SetTimestampAsync(DateTime timestamp, CancellationToken cancellationToken = default)
    {
        var blob = new LastRanBlobObject(timestamp.ToOADate());
        return await blobStorageService.SaveAsync(censusOptions.Value.LastRunBlobName, blob, cancellationToken);
    }
}
