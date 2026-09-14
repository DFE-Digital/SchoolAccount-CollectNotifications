using Microsoft.Extensions.Options;
using MiniExcelLibs;
using SchoolAccount.CollectionNotifications.Interfaces;
using SchoolAccount.CollectionNotifications.Models.Dtos;
using SchoolAccount.CollectionNotifications.Models.Options;

namespace SchoolAccount.CollectionNotifications.Stores.Enrollment;

public class EnrollmentCsvStore(
    IOptions<EnrollmentExcelOptions> options
) : IEnrollmentStore
{
    public async Task<List<EnrolledRecipient>> ReadAsync(CancellationToken cancellationToken = default)
    {
        var recipients = new List<EnrolledRecipient>();
        
        await using var stream = File.OpenRead(options.Value.FilePath);
        foreach (var row in stream.Query<EnrolledRecipient>(sheetName: options.Value.SheetName))
        {
            cancellationToken.ThrowIfCancellationRequested();
            recipients.Add(row);
        }

        return recipients;
    }
}