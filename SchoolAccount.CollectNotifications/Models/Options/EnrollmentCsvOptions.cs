using System.ComponentModel.DataAnnotations;

namespace SchoolAccount.CollectNotifications.Models.Options;

public sealed class EnrollmentCsvOptions
{
    public const string SectionName = "Enrollment:Csv";

    [Required, MinLength(1)]
    public string FilePath { get; init; } = "";
    
    public string? SheetName { get; init; }

    public string StartCell { get; init; } = "A1";
}