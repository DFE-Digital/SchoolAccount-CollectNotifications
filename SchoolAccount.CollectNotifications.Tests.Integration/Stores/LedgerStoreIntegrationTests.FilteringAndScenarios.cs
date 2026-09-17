using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Tests.Integration.Helpers;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.CollectReturnStatusBuilder;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Stores;

public partial class LedgerStoreIntegrationTests
{
    [Fact]
    public async Task GetWhatHasChangedAsync_should_return_empty_result_when_requested_lae_stab_keys_list_is_empty()
    {
        // Arrange
        var store = CreateLedgerStore();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await store.GetWhatHasChangedAsync(lastRunDate, [], limitToApprovedStatuses: false);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetWhatHasChangedAsync_should_filter_results_to_match_only_requested_lae_stab_keys()
    {
        // Arrange
        var store = CreateLedgerStore();
        var requestedLaeStab = CreateTrackedLaeStab();
        var unrequestedLaeStab = CreateTrackedLaeStab();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        var requestedRecord = ACollectReturnStatus()
            .WithLaeStab(requestedLaeStab)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        var unrequestedRecord = ACollectReturnStatus()
            .WithLaeStab(unrequestedLaeStab)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        await TestDatabaseHelper.InsertReturnStatusesAsync([requestedRecord, unrequestedRecord]);

        // Act
        var result =
            await store.GetWhatHasChangedAsync(lastRunDate, [requestedLaeStab], limitToApprovedStatuses: false);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value.Single().LaeStab.ShouldBe(requestedLaeStab);
    }

    [Fact]
    public async Task GetWhatHasChangedAsync_should_filter_to_approved_statuses_when_limit_to_approved_statuses_is_true()
    {
        // Arrange
        var store = CreateLedgerStore([ReturnStatusCodes.Authorised, ReturnStatusCodes.Approved]);
        var approvedLaeStab = CreateTrackedLaeStab();
        var unapprovedLaeStab = CreateTrackedLaeStab();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        // School 1: changed to Authorised (approved)
        var school1Baseline = ACollectReturnStatus()
            .WithLaeStab(approvedLaeStab)
            .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithUpdatedAt(lastRunDate.AddDays(-1))
            .Build();

        var school1Current = ACollectReturnStatus()
            .WithLaeStab(approvedLaeStab)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        // School 2: changed to Rejected (unapproved, baseline is also unapproved)
        var school2Baseline = ACollectReturnStatus()
            .WithLaeStab(unapprovedLaeStab)
            .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithUpdatedAt(lastRunDate.AddDays(-1))
            .Build();

        var school2Current = ACollectReturnStatus()
            .WithLaeStab(unapprovedLaeStab)
            .WithReturnStatusCode(ReturnStatusCodes.Rejected)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        await TestDatabaseHelper.InsertReturnStatusesAsync([
            school1Baseline, school1Current,
            school2Baseline, school2Current
        ]);

        // Act
        var result = await store.GetWhatHasChangedAsync(
            lastRunDate,
            [approvedLaeStab, unapprovedLaeStab],
            limitToApprovedStatuses: true);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value.Single().LaeStab.ShouldBe(approvedLaeStab);
    }

    [Fact]
    public async Task GetWhatHasChangedAsync_should_return_all_changed_statuses_when_limit_to_approved_statuses_is_false()
    {
        // Arrange
        var store = CreateLedgerStore([ReturnStatusCodes.Authorised]);
        var laeStab1 = CreateTrackedLaeStab();
        var laeStab2 = CreateTrackedLaeStab();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        var school1 = ACollectReturnStatus()
            .WithLaeStab(laeStab1)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        var school2 = ACollectReturnStatus()
            .WithLaeStab(laeStab2)
            .WithReturnStatusCode(ReturnStatusCodes.Rejected)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        await TestDatabaseHelper.InsertReturnStatusesAsync([school1, school2]);

        // Act
        var result = await store.GetWhatHasChangedAsync(
            lastRunDate,
            [laeStab1, laeStab2],
            limitToApprovedStatuses: false);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value.Select(x => x.LaeStab).ShouldBe([laeStab1, laeStab2], ignoreOrder: true);
    }

    [Fact]
    public async Task GetWhatHasChangedAsync_should_handle_mixed_scenarios_correctly_across_multiple_schools()
    {
        // Arrange
        var store = CreateLedgerStore([ReturnStatusCodes.Authorised, ReturnStatusCodes.Approved]);
        var newSchool = CreateTrackedLaeStab();
        var changedSchool = CreateTrackedLaeStab();
        var unchangedSchool = CreateTrackedLaeStab();
        var excludedSchool = CreateTrackedLaeStab();
        var lastRunDate = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

        // 1. New school (no baseline) -> Approved
        var newSchoolRecord = ACollectReturnStatus()
            .WithLaeStab(newSchool)
            .WithReturnStatusCode(ReturnStatusCodes.Approved)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        // 2. Changed school: LoadedAndValidated -> Authorised
        var changedBaseline = ACollectReturnStatus()
            .WithLaeStab(changedSchool)
            .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithUpdatedAt(lastRunDate.AddDays(-1))
            .Build();
        var changedCurrent = ACollectReturnStatus()
            .WithLaeStab(changedSchool)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(2))
            .Build();

        // 3. Unchanged school: Authorised -> Authorised
        var unchangedBaseline = ACollectReturnStatus()
            .WithLaeStab(unchangedSchool)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddDays(-1))
            .Build();
        var unchangedCurrent = ACollectReturnStatus()
            .WithLaeStab(unchangedSchool)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        // 4. Excluded school (not in search keys): LoadedAndValidated -> Authorised
        var excludedBaseline = ACollectReturnStatus()
            .WithLaeStab(excludedSchool)
            .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
            .WithUpdatedAt(lastRunDate.AddDays(-1))
            .Build();
        var excludedCurrent = ACollectReturnStatus()
            .WithLaeStab(excludedSchool)
            .WithReturnStatusCode(ReturnStatusCodes.Authorised)
            .WithUpdatedAt(lastRunDate.AddHours(1))
            .Build();

        await TestDatabaseHelper.InsertReturnStatusesAsync([
            newSchoolRecord,
            changedBaseline, changedCurrent,
            unchangedBaseline, unchangedCurrent,
            excludedBaseline, excludedCurrent
        ]);

        // Act
        var result = await store.GetWhatHasChangedAsync(
            lastRunDate,
            [newSchool, changedSchool, unchangedSchool],
            limitToApprovedStatuses: true);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);

        var changedResult = result.Value.Single(x => x.LaeStab == changedSchool);
        changedResult.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Authorised);
        changedResult.InitialReturnStatusCode.ShouldBe(ReturnStatusCodes.LoadedAndValidated);
        changedResult.PreviousReturnStatusCode.ShouldBe(ReturnStatusCodes.LoadedAndValidated);

        var newResult = result.Value.Single(x => x.LaeStab == newSchool);
        newResult.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Approved);
        newResult.InitialReturnStatusCode.ShouldBeNull();
        newResult.PreviousReturnStatusCode.ShouldBeNull();
    }
}
