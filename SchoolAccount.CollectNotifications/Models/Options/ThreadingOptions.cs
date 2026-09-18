namespace SchoolAccount.CollectNotifications.Models.Options;

public sealed class ThreadingOptions
{
    public const string SectionName = "Threading";
    
    public int BatchAmount { get; init; } = 50;
    public int BatchWaitAmountInMs { get; init; } = 1000;
    public int ItemWaitAmountInMs { get; init; } = 50;
}