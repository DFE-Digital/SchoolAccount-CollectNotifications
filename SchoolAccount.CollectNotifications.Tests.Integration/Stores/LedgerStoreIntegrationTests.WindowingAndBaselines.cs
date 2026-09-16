using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Tests.Integration.Helpers;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.CollectReturnStatusBuilder;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Stores;

public partial class LedgerStoreIntegrationTests
{
    [Fact]
    public async Task GetWhatHasChangedAsync_should_return_status_with_null_baseline_when_new_return_occurs_after_last_run_date()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        var newRecord = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithSchoolName("St Mary's Primary")
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(2))
            .Build();

        await TestDatabaseHelper.InsertReturnStatusesAsync([newRecord]);

        // Act
        var result = await store.GetWhatHasChangedAsync(lastRunDate, [laeStab], limitToApprovedStatuses: false);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);

        var change = result.Value.Single();
        change.LaeStab.ShouldBe(laeStab);
        change.SchoolName.ShouldBe("St Mary's Primary");
        change.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Authorised);
        change.PreviousReturnStatusCode.ShouldBeNull();
        change.InitialReturnStatusCode.ShouldBeNull();
    }

    [Fact]
    public async Task GetWhatHasChangedAsync_should_return_latest_status_with_previous_baseline_when_status_changed_since_last_run()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        var baseline = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithSchoolName("Oakfield Academy")
            .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithUpdatedAt(lastRunDate.AddDays(-1))
            .Build();

        var updated = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithSchoolName("Oakfield Academy")
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        await TestDatabaseHelper.InsertReturnStatusesAsync([baseline, updated]);

        // Act
        var result = await store.GetWhatHasChangedAsync(lastRunDate, [laeStab], limitToApprovedStatuses: false);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);

        var change = result.Value.Single();
        change.LaeStab.ShouldBe(laeStab);
        change.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Authorised);
        change.PreviousReturnStatusCode.ShouldBe(ReturnStatusCodes.LoadedAndValidated);
        change.InitialReturnStatusCode.ShouldBe(ReturnStatusCodes.LoadedAndValidated);
    }

    [Fact]
    public async Task GetWhatHasChangedAsync_should_return_empty_list_when_status_in_window_is_identical_to_baseline()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        var baseline = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddDays(-1))
            .Build();

        var unchangedUpdate = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        await TestDatabaseHelper.InsertReturnStatusesAsync([baseline, unchangedUpdate]);

        // Act
        var result = await store.GetWhatHasChangedAsync(lastRunDate, [laeStab], limitToApprovedStatuses: false);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetWhatHasChangedAsync_should_pick_latest_status_in_window_and_baseline_before_last_run_date()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        var baseline = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.NoData)
            .WithUpdatedAt(lastRunDate.AddDays(-2))
            .Build();

        var firstInWindow = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        var middleInWindow = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.Rejected)
            .WithUpdatedAt(lastRunDate.AddHours(2))
            .Build();

        var lastInWindow = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(3))
            .Build();

        await TestDatabaseHelper.InsertReturnStatusesAsync([baseline, firstInWindow, middleInWindow, lastInWindow]);

        // Act
        var result = await store.GetWhatHasChangedAsync(lastRunDate, [laeStab], limitToApprovedStatuses: false);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);

        var change = result.Value.Single();
        change.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Authorised);
        change.PreviousReturnStatusCode.ShouldBe(ReturnStatusCodes.NoData);
        change.InitialReturnStatusCode.ShouldBe(ReturnStatusCodes.NoData);
    }

    [Fact]
    public async Task GetWhatHasChangedAsync_should_pick_initial_and_previous_baselines_when_multiple_baselines_exist_before_last_run_date()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        var oldBaseline = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.NoData)
            .WithUpdatedAt(lastRunDate.AddDays(-5))
            .Build();

        var latestBaseline = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithUpdatedAt(lastRunDate.AddDays(-1))
            .Build();

        var currentUpdate = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        await TestDatabaseHelper.InsertReturnStatusesAsync([oldBaseline, latestBaseline, currentUpdate]);

        // Act
        var result = await store.GetWhatHasChangedAsync(lastRunDate, [laeStab], limitToApprovedStatuses: false);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);

        var change = result.Value.Single();
        change.PreviousReturnStatusCode.ShouldBe(ReturnStatusCodes.LoadedAndValidated);
        change.InitialReturnStatusCode.ShouldBe(ReturnStatusCodes.NoData);
        change.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Authorised);
    }

    [Fact]
    public async Task GetWhatHasChangedAsync_should_return_empty_when_all_records_are_prior_to_last_run_date()
    {
        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        var oldRecord1 = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.NoData)
            .WithUpdatedAt(lastRunDate.AddDays(-3))
            .Build();

        var oldRecord2 = ACollectReturnStatus()
            .WithLaeStab(laeStab)
            .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithUpdatedAt(lastRunDate.AddDays(-1))
            .Build();

        await TestDatabaseHelper.InsertReturnStatusesAsync([oldRecord1, oldRecord2]);

        // Act
        var result = await store.GetWhatHasChangedAsync(lastRunDate, [laeStab], limitToApprovedStatuses: false);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }
}
