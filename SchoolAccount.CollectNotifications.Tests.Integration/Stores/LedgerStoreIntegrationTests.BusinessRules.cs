using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Tests.Integration.Helpers;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.CollectReturnStatusBuilder;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Stores;

public partial class LedgerStoreIntegrationTests
{
    [Fact]
    public async Task When_getting_what_has_changed_it_should_notify_every_qualifying_transition_a_return_made_between_runs()
    {
        // A return that reaches Approved and then moves off it again before the service next runs.
        // Both of those transitions are notifiable, and a missed run should not mean we skip
        // qualifying changes we have already captured:
        //
        //   Loaded_and_Validated -> Approved   any other status -> Approved   send
        //   Approved -> Rejected               Approved -> any other status   send
        //
        // The two either side of them are not notifiable and should stay filtered out.

        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        await RegisterAsync(laeStab);
        var lastRunDate = new DateTime(2026, 9, 16, 0, 0, 0, DateTimeKind.Utc);

        List<CollectReturnStatus> history =
        [
            StatusAt(laeStab, ReturnStatusCodes.NoData, new DateTime(2026, 9, 10, 11, 0, 0, DateTimeKind.Utc)),
            StatusAt(laeStab, ReturnStatusCodes.LoadedAndValidated, new DateTime(2026, 9, 17, 11, 0, 0, DateTimeKind.Utc)),
            StatusAt(laeStab, ReturnStatusCodes.Approved, new DateTime(2026, 9, 18, 11, 0, 0, DateTimeKind.Utc)),
            StatusAt(laeStab, ReturnStatusCodes.Rejected, new DateTime(2026, 9, 19, 11, 0, 0, DateTimeKind.Utc)),
            StatusAt(laeStab, ReturnStatusCodes.AmendedBySource, new DateTime(2026, 9, 20, 11, 0, 0, DateTimeKind.Utc)),
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history, _cancellationToken);

        // Act
        var result = await store.GetWhatHasChangedAsync(lastRunDate, cancellationToken: _cancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldAllBe(x => x.LaeStab == laeStab);
        result.Value.Count.ShouldBe(2);

        var reachedApproved = result.Value[0];
        reachedApproved.PreviousReturnStatusCode.ShouldBe(ReturnStatusCodes.LoadedAndValidated);
        reachedApproved.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Approved);

        var cameOffApproved = result.Value[1];
        cameOffApproved.PreviousReturnStatusCode.ShouldBe(ReturnStatusCodes.Approved);
        cameOffApproved.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Rejected);
    }

    [Fact]
    public async Task When_getting_what_has_changed_it_should_not_notify_a_return_whose_transitions_are_all_outside_the_rules()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        await RegisterAsync(laeStab);
        var lastRunDate = new DateTime(2026, 9, 16, 0, 0, 0, DateTimeKind.Utc);

        List<CollectReturnStatus> history =
        [
            StatusAt(laeStab, ReturnStatusCodes.NoData, new DateTime(2026, 9, 10, 11, 0, 0, DateTimeKind.Utc)),
            StatusAt(laeStab, ReturnStatusCodes.LoadedAndValidated, new DateTime(2026, 9, 17, 11, 0, 0, DateTimeKind.Utc)),
            StatusAt(laeStab, ReturnStatusCodes.Rejected, new DateTime(2026, 9, 19, 11, 0, 0, DateTimeKind.Utc)),
            StatusAt(laeStab, ReturnStatusCodes.AmendedBySource, new DateTime(2026, 9, 20, 11, 0, 0, DateTimeKind.Utc)),
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history, _cancellationToken);

        // Act
        var result = await store.GetWhatHasChangedAsync(lastRunDate, cancellationToken: _cancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    private static CollectReturnStatus StatusAt(string laeStab, ReturnStatusCodes status, DateTime updatedAt) =>
        ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithSchoolName("A test School")
            .WithReturnStatusCode(status)
            .WithUpdatedAt(updatedAt);
}
