using System.ComponentModel.DataAnnotations;

namespace SchoolAccount.CollectNotifications.Models.Options;

public class GovNotifyOptions
{
    public const string SectionName = "GovNotify";
    
    [Required]
    public required string ApiKey { get; init; }
    
    [EmailAddress]
    public string? FromAddress { get; init; }
}