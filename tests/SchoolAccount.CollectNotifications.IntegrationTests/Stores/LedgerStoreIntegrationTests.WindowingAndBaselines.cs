using SchoolAccount.CollectNotifications.IntegrationTests.Helpers;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;
using static SchoolAccount.CollectNotifications.TestCommon.Builders.CollectReturnStatusBuilder;

namespace SchoolAccount.CollectNotifications.IntegrationTests.Stores;

public partial class LedgerStoreIntegrationTests
{
    private static readonly DateTime LastRunDate = new(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task When_getting_what_has_changed_it_should_return_a_null_previous_status_for_a_returns_first_ever_row()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        await RegisterAsync(laeStab);

        List<CollectReturnStatus> history =
        [
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithSchoolName("St Mary's Primary")
                .WithReturnStatusCode(ReturnStatusCodes.Authorised)
                .WithUpdatedAt(LastRunDate.AddHours(2)),
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history, _cancellationToken);

        // Act
        var result = await store.GetWhatHasChangedAsync(
            LastRunDate,
            cancellationToken: _cancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var change = result.Value.ShouldHaveSingleItem();
        change.LaeStab.ShouldBe(laeStab);
        change.SchoolName.ShouldBe("St Mary's Primary");
        change.Email.ShouldBe(DefaultEmail);
        change.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Authorised);
        change.PreviousReturnStatusCode.ShouldBeNull();
    }

    [Fact]
    public async Task When_getting_what_has_changed_it_should_return_the_previous_status_when_the_status_changed()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        await RegisterAsync(laeStab);

        List<CollectReturnStatus> history =
        [
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
                .WithUpdatedAt(LastRunDate.AddDays(-1)),
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.Authorised)
                .WithUpdatedAt(LastRunDate.AddHours(1)),
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history, _cancellationToken);

        // Act
        var result = await store.GetWhatHasChangedAsync(
            LastRunDate,
            cancellationToken: _cancellationToken
        );

        // Assert
        var change = result.Value.ShouldHaveSingleItem();
        change.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Authorised);
        change.PreviousReturnStatusCode.ShouldBe(ReturnStatusCodes.LoadedAndValidated);
    }

    [Fact]
    public async Task When_getting_what_has_changed_it_should_return_nothing_when_the_latest_status_matches_the_one_before_it()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        await RegisterAsync(laeStab);

        List<CollectReturnStatus> history =
        [
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.Authorised)
                .WithUpdatedAt(LastRunDate.AddDays(-1)),
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.Authorised)
                .WithUpdatedAt(LastRunDate.AddHours(1)),
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history, _cancellationToken);

        // Act
        var result = await store.GetWhatHasChangedAsync(
            LastRunDate,
            cancellationToken: _cancellationToken
        );

        // Assert
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task When_getting_what_has_changed_it_should_filter_out_transitions_where_neither_end_is_notifiable()
    {
        // The status moves several times after the last run. Each move is compared against the row
        // before it, and only the one ending on a notifiable status survives the filter.

        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        await RegisterAsync(laeStab);

        List<CollectReturnStatus> history =
        [
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.NoData)
                .WithUpdatedAt(LastRunDate.AddDays(-2)),
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
                .WithUpdatedAt(LastRunDate.AddHours(1)),
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.Rejected)
                .WithUpdatedAt(LastRunDate.AddHours(2)),
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.Authorised)
                .WithUpdatedAt(LastRunDate.AddHours(3)),
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history, _cancellationToken);

        // Act
        var result = await store.GetWhatHasChangedAsync(
            LastRunDate,
            cancellationToken: _cancellationToken
        );

        // Assert
        var change = result.Value.ShouldHaveSingleItem();
        change.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Authorised);
        change.PreviousReturnStatusCode.ShouldBe(ReturnStatusCodes.Rejected);
    }

    [Fact]
    public async Task When_getting_what_has_changed_it_should_return_nothing_when_every_row_predates_the_last_run_date()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        await RegisterAsync(laeStab);

        List<CollectReturnStatus> history =
        [
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.NoData)
                .WithUpdatedAt(LastRunDate.AddDays(-3)),
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.Authorised)
                .WithUpdatedAt(LastRunDate.AddDays(-1)),
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history, _cancellationToken);

        // Act
        var result = await store.GetWhatHasChangedAsync(
            LastRunDate,
            cancellationToken: _cancellationToken
        );

        // Assert
        result.Value.ShouldBeEmpty();
    }
}
