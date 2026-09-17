namespace SchoolAccount.CollectNotifications.Models.Dtos;

public sealed class EnrolledRecipient
{
    public string? LaeStab { get; init; }
    public string? Email { get; init; }
    
    public bool IsValid => !string.IsNullOrWhiteSpace(LaeStab) && !string.IsNullOrWhiteSpace(Email);
}