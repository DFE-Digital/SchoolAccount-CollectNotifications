using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Exceptions;
using Notify.Interfaces;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Services;

public class GovNotifyService(
    IOptions<GovNotifyOptions> settings,
    IAsyncNotificationClient? client = null
) : IGovNotifyService
{
    private readonly IAsyncNotificationClient _client = client ?? new NotificationClient(settings.Value.ApiKey);

    public async Task<Result<NotificationResult>> SendMessage(
        string templateId, 
        string recipient, 
        Dictionary<string, dynamic>? properties = null, 
        EmailOptions? options = null)
    {
        try
        {
            var result = await _client.SendEmailAsync(
                recipient, 
                templateId, 
                properties,
                clientReference: options?.Reference,
                emailReplyToId: options?.ReplyTo ?? settings.Value.FromAddress);
            
            return Result.Success(new NotificationResult { Outcome = result });
        }
        catch (NotifyClientException ex)
        {
            return Result.Failure<NotificationResult>(ex.Message);
        }
    }
}
