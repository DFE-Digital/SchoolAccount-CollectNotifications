using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Exceptions;
using Notify.Interfaces;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Options;
using static System.StringComparison;

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
        // Invalid email address
        catch (NotifyClientException ex) when (ex.Message.Contains("Not a valid email address",
                                                   OrdinalIgnoreCase))
        {
            return Result.Warning(new NotificationResult(), "The recipient email address is invalid.");
        }
        // Team-only test API key restriction
        catch (NotifyClientException ex) when (ex.Message.Contains("can only send to team members",
                                                   OrdinalIgnoreCase))
        {
            return Result.Warning(new NotificationResult(), "Cannot send to non-team members with a test API key.");
        }
        // Rate limit / daily limit reached
        catch (NotifyClientException ex) when (ex.Message.StartsWith("Status code 429",
                                                   OrdinalIgnoreCase))
        {
            return Result.Failure<NotificationResult>("Gov Notify rate limit exceeded.");
        }
        // Authentication failure
        catch (NotifyAuthException ex)
        {
            return Result.Failure<NotificationResult>($"Authentication failed: {ex.Message}");
        }
        // Fallback
        catch (NotifyClientException ex)
        {
            return Result.Failure<NotificationResult>(ex.Message);
        }
    }
}
