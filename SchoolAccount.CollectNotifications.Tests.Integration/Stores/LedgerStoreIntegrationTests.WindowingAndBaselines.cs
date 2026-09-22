using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Tests.Integration.Helpers;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.CollectReturnStatusBuilder;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Stores;

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
                .WithUpdatedAt(LastRunDate.AddHours(2))
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history);

        // Act
        var result = await store.GetWhatHasChangedAsync(LastRunDate);

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
                .WithUpdatedAt(LastRunDate.AddHours(1))
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history);

        // Act
        var result = await store.GetWhatHasChangedAsync(LastRunDate);

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
                .WithUpdatedAt(LastRunDate.AddHours(1))
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history);

        // Act
        var result = await store.GetWhatHasChangedAsync(LastRunDate);

        // Assert
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task When_getting_what_has_changed_it_should_compare_the_latest_row_against_the_one_immediately_before_it()
    {
        // The status moves several times after the last run. The comparison is against the row
        // immediately before the latest one, not against where the return stood at the last run.

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
                .WithUpdatedAt(LastRunDate.AddHours(3))
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history);

        // Act
        var result = await store.GetWhatHasChangedAsync(LastRunDate);

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
                .WithUpdatedAt(LastRunDate.AddDays(-1))
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history);

        // Act
        var result = await store.GetWhatHasChangedAsync(LastRunDate);

        // Assert
        result.Value.ShouldBeEmpty();
    }
}
