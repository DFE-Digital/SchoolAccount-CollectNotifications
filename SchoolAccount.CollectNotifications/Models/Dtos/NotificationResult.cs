using Notify.Models.Responses;

namespace SchoolAccount.CollectNotifications.Models.Dtos;

public class NotificationResult
{
    public EmailNotificationResponse? Outcome { get; init; }
}