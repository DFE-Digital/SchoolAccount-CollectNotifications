using Notify.Models.Responses;

namespace SchoolAccount.CollectNotifications.Models.Dtos;

public sealed class NotificationResult
{
    public EmailNotificationResponse? Outcome { get; init; }
}