using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Exceptions;
using Notify.Interfaces;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Services;

public partial class GovNotifyService(
    IOptions<GovNotifyOptions> settings,
    IAsyncNotificationClient? client = null
) : IGovNotifyService
{
    private readonly IAsyncNotificationClient _client = client ?? new NotificationClient(settings.Value.ApiKey);

    public async Task<Result<NotificationResult>> SendMessage(
        string templateId,
        string recipient,
        Dictionary<string, dynamic>? properties = null)
    {
        try
        {
            var result = await _client.SendEmailAsync(recipient, templateId, properties);

            return Result.Success(new NotificationResult { Outcome = result });
        }
        catch (NotifyClientException exception)
        {
            return StatusCodeOf(exception.Message) == 400
                ? Result.Warning(new NotificationResult(), exception.Message)
                : Result.Failure<NotificationResult>(exception.Message);
        }
        catch (NotifyAuthException exception)
        {
            return Result.Failure<NotificationResult>(exception.Message);
        }
    }

    private static int? StatusCodeOf(string message) =>
        StatusCodePrefix().Match(message) is { Success: true } match
            ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            : null;

    [GeneratedRegex(@"^Status code (\d+)")]
    private static partial Regex StatusCodePrefix();
}
