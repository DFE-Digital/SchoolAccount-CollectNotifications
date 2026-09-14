using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Exceptions;
using SchoolAccount.CollectionNotifications.Models;
using SchoolAccount.CollectionNotifications.Models.Dtos;
using SchoolAccount.CollectionNotifications.Models.Options;

namespace SchoolAccount.CollectionNotifications.Services;

public class GovNotifyService(
    IOptions<GovNotifyOptions> settings
)
{
    private readonly NotificationClient _client = new(settings.Value.ApiKey);

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
            
            return Result.Success(new NotificationResult { Outcome = result});
        }
        catch (NotifyClientException ex)
        {
            return Result.Failure<NotificationResult>(ex.Message);
        }
    }
}