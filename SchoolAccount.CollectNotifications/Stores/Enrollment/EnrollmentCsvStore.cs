using Microsoft.Extensions.Options;
using MiniExcelLibs;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Stores.Enrollment;

public class EnrollmentCsvStore(
    IOptions<EnrollmentCsvOptions> options,
    IBlobStorageService blobStorageService
) : IEnrollmentStore
{
    public async Task<Result<List<EnrolledRecipient>>> ListAsync(CancellationToken cancellationToken = default)
    {
        Stream stream;

        if (!string.IsNullOrEmpty(options.Value.FilePath))
        {
            stream = File.OpenRead(options.Value.FilePath);
        }
        else if (!string.IsNullOrEmpty(options.Value.BlobName))
        {
            var file = await blobStorageService.GetFileAsync(options.Value.BlobName, cancellationToken);

            if (file.IsFailure || file.Value is null)
            {
                return Result.Failure<List<EnrolledRecipient>>(file.Error);
            }
            
            stream = file.Value;
        }
        else
        {
            throw new ApplicationException("Enrollment CSV not initialised correctly");
        }

        var records = stream
            .Query<EnrolledRecipient>(
                sheetName: options.Value.SheetName,
                startCell: options.Value.StartCell)
            .Where(x => x.IsValid)
            .ToList();
        
        stream.Close();
        await stream.DisposeAsync();
        
        return Result.Success(records);
    }
}