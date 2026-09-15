using Microsoft.Extensions.Options;
using Notify.Exceptions;
using Notify.Interfaces;
using Notify.Models.Responses;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services;

namespace SchoolAccount.CollectNotifications.Tests.Services;

public class GovNotifyServiceTests
{
    private const string TemplateId = "test-template-id";
    private const string Recipient = "user@school.sch.uk";

    private readonly IAsyncNotificationClient _notificationClient = Substitute.For<IAsyncNotificationClient>();

    private readonly IOptions<GovNotifyOptions> _options = Options.Create(new GovNotifyOptions
    {
        ApiKey = "test-api-key-12345",
        FromAddress = "default-reply-to-id"
    });

    private readonly GovNotifyService _sut;

    public GovNotifyServiceTests()
    {
        _sut = new GovNotifyService(_options, _notificationClient);
    }

    [Fact]
    public async Task SendMessage_should_return_success_when_email_is_sent_successfully()
    {
        // Arrange
        var properties = new Dictionary<string, dynamic> { { "status", "10" }, { "school_name", "Test School" } };
        var response = new EmailNotificationResponse { id = "notification-uuid-123" };

        _notificationClient
            .SendEmailAsync(Recipient, TemplateId, properties, null, "default-reply-to-id")
            .Returns(response);

        // Act
        var result = await _sut.SendMessage(TemplateId, Recipient, properties);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Outcome.ShouldBe(response);
    }

    [Fact]
    public async Task SendMessage_should_use_custom_reply_to_and_reference_when_options_are_provided()
    {
        // Arrange
        var options = new EmailOptions
        {
            Reference = "custom-client-ref",
            ReplyTo = "custom-reply-to-id"
        };
        var response = new EmailNotificationResponse { id = "notification-uuid-456" };

        _notificationClient
            .SendEmailAsync(Recipient, TemplateId, null, "custom-client-ref", "custom-reply-to-id")
            .Returns(response);

        // Act
        var result = await _sut.SendMessage(TemplateId, Recipient, options: options);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Outcome.ShouldBe(response);
    }

    [Fact]
    public async Task SendMessage_should_return_failure_when_notify_client_throws_exception()
    {
        // Arrange
        const string invalidEmail = "invalid-email";

        _notificationClient
            .SendEmailAsync(invalidEmail, TemplateId, Arg.Any<Dictionary<string, dynamic>>(), Arg.Any<string>(),
                Arg.Any<string>())
            .Returns<EmailNotificationResponse>(_ =>
                throw new NotifyClientException("Validation error: email address is not valid"));

        // Act
        var result = await _sut.SendMessage(TemplateId, invalidEmail);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("Validation error: email address is not valid");
    }
}
