using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Stores;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.CollectReturnStatusBuilder;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Stores;

/// <summary>
/// The business rules from the census status change story
///
///   Start status                        End status                          Send
///   any other status                    Approved                            yes
///   any other status                    Authorised                          yes
///   Authorised                          any other status                    yes
///   Approved                            any other status                    yes
///   not in (Authorised, Approved)       not in (Authorised, Approved)       no
/// </summary>
public class NotificationRulesTests
{
    private static readonly List<ReturnStatusCodes> NotifiableStatuses =
        [ReturnStatusCodes.Approved, ReturnStatusCodes.Authorised];

    [Theory]
    [InlineData(ReturnStatusCodes.LoadedAndValidated, ReturnStatusCodes.Approved, true)]
    [InlineData(ReturnStatusCodes.Submitted, ReturnStatusCodes.Authorised, true)]
    [InlineData(ReturnStatusCodes.Authorised, ReturnStatusCodes.Rejected, true)]
    [InlineData(ReturnStatusCodes.Approved, ReturnStatusCodes.AmendedBySource, true)]
    [InlineData(ReturnStatusCodes.LoadedAndValidated, ReturnStatusCodes.Rejected, false)]
    public void Should_follow_the_business_rules_for_a_single_transition(
        ReturnStatusCodes startStatus,
        ReturnStatusCodes endStatus,
        bool shouldSend)
    {
        // Arrange
        var change = ACollectReturnStatus()
            .WithPreviousReturnStatusCode(startStatus)
            .WithReturnStatusCode(endStatus)
            .Build();

        var list = new List<ComparableCollectReturnStatus> { change };

        // Act
        var result = list.RestrictToApprovedStatuses(NotifiableStatuses).ToList();

        // Assert
        result.Any().ShouldBe(shouldSend);
    }

    [Fact]
    public void Should_not_send_when_neither_end_of_the_transition_is_notifiable_even_if_an_earlier_status_was()
    {
        // Arrange
        var change = ACollectReturnStatus()
            .WithPreviousReturnStatusCode(ReturnStatusCodes.Rejected)
            .WithReturnStatusCode(ReturnStatusCodes.AmendedBySource)
            .WithInitialReturnStatusCode(ReturnStatusCodes.Approved)
            .Build();

        var list = new List<ComparableCollectReturnStatus> { change };

        // Act
        var result = list.RestrictToApprovedStatuses(NotifiableStatuses).ToList();

        // Assert
        result.ShouldBeEmpty();
    }
}
