using System.ComponentModel.DataAnnotations;

namespace SchoolAccount.CollectionNotifications.Models.Options;

public sealed class EnrollmentExcelOptions
{
    public const string SectionName = "Enrollment:Excel";

    [Required, MinLength(1)]
    public string FilePath { get; init; } = "";
    
    public string? SheetName { get; init; }
}