using Microsoft.Extensions.Options;
using MiniExcelLibs;
using SchoolAccount.CollectionNotifications.Interfaces;
using SchoolAccount.CollectionNotifications.Models.Dtos;
using SchoolAccount.CollectionNotifications.Models.Options;

namespace SchoolAccount.CollectionNotifications.Stores.Enrollment;

public class EnrollmentCsvStore(
    IOptions<EnrollmentCsvOptions> options
) : IEnrollmentStore
{
    public async Task<List<EnrolledRecipient>> ReadAsync(CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(options.Value.FilePath);
        return stream
            .Query<EnrolledRecipient>(
                sheetName: options.Value.SheetName,
                startCell: options.Value.StartCell)
            .ToList();
    }
}