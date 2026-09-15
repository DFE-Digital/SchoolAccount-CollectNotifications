using Microsoft.Extensions.Options;
using MiniExcelLibs;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Stores.Enrollment;

public class EnrollmentCsvStore(
    IOptions<EnrollmentCsvOptions> options
) : IEnrollmentStore
{
    public async Task<Result<List<EnrolledRecipient>>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(options.Value.FilePath);
        var records = stream
            .Query<EnrolledRecipient>(
                sheetName: options.Value.SheetName,
                startCell: options.Value.StartCell)
            .Where(x => x.IsValid)
            .ToList();
        
        return Result.Success(records);
    }
}