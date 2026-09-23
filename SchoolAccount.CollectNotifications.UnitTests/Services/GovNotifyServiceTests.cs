using Microsoft.Extensions.Options;
using Notify.Exceptions;
using Notify.Interfaces;
using Notify.Models.Responses;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Services;

public class GovNotifyServiceTests
{
    private const string TemplateId = "test-template-id";
    private const string Recipient = "user@school.sch.uk";

    private readonly IAsyncNotificationClient _notificationClient =
        Substitute.For<IAsyncNotificationClient>();

    private readonly IOptions<GovNotifyOptions> _options = Options.Create(
        new GovNotifyOptions { ApiKey = "test-api-key-12345" }
    );

    private readonly GovNotifyService _sut;

    public GovNotifyServiceTests()
    {
        _sut = new GovNotifyService(_options, _notificationClient);
    }

    [Fact]
    public async Task When_sending_a_message_it_should_return_success_when_email_is_sent_successfully()
    {
        // Arrange
        var properties = new Dictionary<string, dynamic>
        {
            { "status", "10" },
            { "school_name", "Test School" },
        };
        var response = new EmailNotificationResponse { id = "notification-uuid-123" };

        _notificationClient.SendEmailAsync(Recipient, TemplateId, properties).Returns(response);

        // Act
        var result = await _sut.SendMessage(TemplateId, Recipient, properties);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Outcome.ShouldBe(response);
    }

    [Theory]
    // In the shape the Notify client actually produces, "Status code {code}. ..."
    [InlineData(
        """Status code 400. The following errors occured [{"error":"ValidationError","message":"email_address Not a valid email address"}]"""
    )]
    [InlineData(
        """Status code 400. The following errors occured [{"error":"BadRequestError","message":"Can't send to this recipient using a team-only API key"}]"""
    )]
    public async Task When_sending_a_message_it_should_warn_rather_than_fail_when_the_problem_is_with_the_recipient(
        string notifyMessage
    )
    {
        // A warning tells the caller to log this one and carry on to everybody else.

        // Arrange
        _notificationClient
            .SendEmailAsync(Recipient, TemplateId, Arg.Any<Dictionary<string, dynamic>>())
            .Returns<EmailNotificationResponse>(_ =>
                throw new NotifyClientException(notifyMessage)
            );

        // Act
        var result = await _sut.SendMessage(TemplateId, Recipient);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBe(notifyMessage);
    }

    [Theory]
    [InlineData(
        """Status code 429. The following errors occured [{"error":"TooManyRequestsError","message":"Exceeded send limits (50) for today"}]"""
    )]
    [InlineData(
        """Status code 500. The following errors occured [{"error":"Exception","message":"Internal server error"}]"""
    )]
    [InlineData("Something without a status code at all")]
    public async Task When_sending_a_message_it_should_fail_when_the_problem_is_with_the_whole_run(
        string notifyMessage
    )
    {
        // A failure stops the run: a rate limit, a server problem, or anything we can't classify.

        // Arrange
        _notificationClient
            .SendEmailAsync(Recipient, TemplateId, Arg.Any<Dictionary<string, dynamic>>())
            .Returns<EmailNotificationResponse>(_ =>
                throw new NotifyClientException(notifyMessage)
            );

        // Act
        var result = await _sut.SendMessage(TemplateId, Recipient);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(notifyMessage);
    }

    [Fact]
    public async Task When_sending_a_message_it_should_fail_when_the_api_key_is_rejected()
    {
        // Arrange
        _notificationClient
            .SendEmailAsync(Recipient, TemplateId, Arg.Any<Dictionary<string, dynamic>>())
            .Returns<EmailNotificationResponse>(_ =>
                throw new NotifyAuthException("Invalid API key")
            );

        // Act
        var result = await _sut.SendMessage(TemplateId, Recipient);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Invalid API key");
    }
}
