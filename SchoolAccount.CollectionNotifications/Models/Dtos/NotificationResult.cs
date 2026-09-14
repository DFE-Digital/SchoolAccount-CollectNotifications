using Notify.Models.Responses;

namespace SchoolAccount.CollectionNotifications.Models.Dtos;

public class NotificationResult
{
    public EmailNotificationResponse? Outcome { get; init; }
}