namespace SchoolAccount.CollectionNotifications.Models.Options;

public class ThreadingOptions
{
    public const string SectionName = "Threading";
    
    public int MaxDegreeOfParallelism { get; init; } = 10;
    public int BatchAmount { get; init; } = 50;
    public int BatchWaitAmountInSec { get; init; } = 10;
    public int ItemWaitAmountInSec { get; init; } = 1;
}