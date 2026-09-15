using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;

namespace SchoolAccount.CollectNotifications.Interfaces;

public interface IGovNotifyService
{
    Task<Result<NotificationResult>> SendMessage(
        string templateId,
        string recipient,
        Dictionary<string, dynamic>? properties = null,
        EmailOptions? options = null);
}
