using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Stores;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.CollectReturnStatusBuilder;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Stores;

public class LedgerStoreExtensionsTests
{
    [Fact]
    public void RestrictToApprovedStatuses_should_return_status_when_current_status_code_is_approved()
    {
        // Arrange
        var approvedStatuses = new List<ReturnStatusCodes> { ReturnStatusCodes.Authorised, ReturnStatusCodes.Approved };

        var item = ACollectReturnStatus()
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithPreviousReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithInitialReturnStatusCode(ReturnStatusCodes.NoData)
            .Build();

        var list = new List<ComparableCollectReturnStatus> { item };

        // Act
        var result = list.RestrictToApprovedStatuses(approvedStatuses).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result.Single().ShouldBe(item);
    }

    [Fact]
    public void RestrictToApprovedStatuses_should_return_status_when_first_return_status_code_is_approved()
    {
        // Arrange
        var approvedStatuses = new List<ReturnStatusCodes> { ReturnStatusCodes.Submitted };

        var item = ACollectReturnStatus()
            .WithReturnStatusCode(ReturnStatusCodes.Rejected)
            .WithPreviousReturnStatusCode(ReturnStatusCodes.Submitted)
            .WithInitialReturnStatusCode(ReturnStatusCodes.NoData)
            .Build();

        var list = new List<ComparableCollectReturnStatus> { item };

        // Act
        var result = list.RestrictToApprovedStatuses(approvedStatuses).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result.Single().ShouldBe(item);
    }

    [Fact]
    public void RestrictToApprovedStatuses_should_return_status_when_baseline_return_status_code_is_approved()
    {
        // Arrange
        var approvedStatuses = new List<ReturnStatusCodes> { ReturnStatusCodes.Approved };

        var item = ACollectReturnStatus()
            .WithReturnStatusCode(ReturnStatusCodes.Rejected)
            .WithPreviousReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithInitialReturnStatusCode(ReturnStatusCodes.Approved)
            .Build();

        var list = new List<ComparableCollectReturnStatus> { item };

        // Act
        var result = list.RestrictToApprovedStatuses(approvedStatuses).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result.Single().ShouldBe(item);
    }

    [Fact]
    public void RestrictToApprovedStatuses_should_filter_out_items_when_no_status_codes_match()
    {
        // Arrange
        var approvedStatuses = new List<ReturnStatusCodes> { ReturnStatusCodes.Authorised, ReturnStatusCodes.Approved };

        var nonMatching = ACollectReturnStatus()
            .WithReturnStatusCode(ReturnStatusCodes.Rejected)
            .WithPreviousReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithInitialReturnStatusCode(ReturnStatusCodes.NoData)
            .Build();

        var matching = ACollectReturnStatus()
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .Build();

        var list = new List<ComparableCollectReturnStatus> { nonMatching, matching };

        // Act
        var result = list.RestrictToApprovedStatuses(approvedStatuses).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result.Single().ShouldBe(matching);
    }

    [Fact]
    public void RestrictToApprovedStatuses_should_handle_null_first_and_baseline_statuses_gracefully()
    {
        // Arrange
        var approvedStatuses = new List<ReturnStatusCodes> { ReturnStatusCodes.Authorised };

        var item = ACollectReturnStatus()
            .WithReturnStatusCode(ReturnStatusCodes.Rejected)
            .WithPreviousReturnStatusCode(null)
            .WithInitialReturnStatusCode(null)
            .Build();

        var list = new List<ComparableCollectReturnStatus> { item };

        // Act
        var result = list.RestrictToApprovedStatuses(approvedStatuses).ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void RestrictToApprovedStatuses_should_return_empty_when_approved_statuses_list_is_empty()
    {
        // Arrange
        var approvedStatuses = new List<ReturnStatusCodes>();

        var item = ACollectReturnStatus()
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithPreviousReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithInitialReturnStatusCode(ReturnStatusCodes.Authorised)
            .Build();

        var list = new List<ComparableCollectReturnStatus> { item };

        // Act
        var result = list.RestrictToApprovedStatuses(approvedStatuses).ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void RestrictToApprovedStatuses_should_return_empty_when_source_collection_is_empty()
    {
        // Arrange
        var approvedStatuses = new List<ReturnStatusCodes> { ReturnStatusCodes.Authorised };
        var list = new List<ComparableCollectReturnStatus>();

        // Act
        var result = list.RestrictToApprovedStatuses(approvedStatuses).ToList();

        // Assert
        result.ShouldBeEmpty();
    }
}
