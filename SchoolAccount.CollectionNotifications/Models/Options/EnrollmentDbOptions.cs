using System.ComponentModel.DataAnnotations;

namespace SchoolAccount.CollectionNotifications.Models.Options;

public sealed class EnrollmentDbOptions
{
    public const string SectionName = "Enrollment:Db";

    [Required, MinLength(1)]
    public string ConnectionString { get; init; } = "";
    
    public int CommandTimeoutSeconds { get; init; } = 30;
}