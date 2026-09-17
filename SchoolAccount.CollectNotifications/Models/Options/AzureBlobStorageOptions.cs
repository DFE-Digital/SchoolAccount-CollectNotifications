namespace SchoolAccount.CollectNotifications.Models.Options;

public sealed class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";
 
    public string? ServiceUri { get; set; }
 
    public string? ConnectionString { get; set; }
 
    public string ContainerName { get; set; } = string.Empty;
    
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString) || !string.IsNullOrWhiteSpace(ServiceUri);
}