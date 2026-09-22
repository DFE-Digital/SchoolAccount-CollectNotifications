using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Tests.Integration.Helpers;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.CollectReturnStatusBuilder;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Stores;

public partial class LedgerStoreIntegrationTests
{
    [Fact]
    public async Task When_getting_what_has_changed_it_should_return_a_change_for_every_registered_recipient()
    {
        // A school with two registered contacts should produce two changes, one each. Joining
        // RegisteredUsers before the ranking instead of after makes this return nothing at all,
        // because rn 1 and rn 2 then land on the same ledger row with different emails.

        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        await RegisterAsync(laeStab, "head@school.sch.uk", "office@school.sch.uk");

        List<CollectReturnStatus> history =
        [
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.Rejected)
                .WithUpdatedAt(LastRunDate.AddDays(-1)),
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.Approved)
                .WithUpdatedAt(LastRunDate.AddHours(1))
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history);

        // Act
        var result = await store.GetWhatHasChangedAsync(LastRunDate);

        // Assert
        result.Value.Count.ShouldBe(2);
        result.Value.Select(x => x.Email)
            .ShouldBe(["head@school.sch.uk", "office@school.sch.uk"], ignoreOrder: true);
        result.Value.ShouldAllBe(x => x.ReturnStatusCode == ReturnStatusCodes.Approved);
        result.Value.ShouldAllBe(x => x.PreviousReturnStatusCode == ReturnStatusCodes.Rejected);
    }

    [Fact]
    public async Task When_getting_what_has_changed_it_should_ignore_schools_with_nobody_registered()
    {
        // Arrange
        var store = CreateLedgerStore();
        var registered = CreateTrackedLaeStab();
        var unregistered = CreateTrackedLaeStab();
        await RegisterAsync(registered);

        List<CollectReturnStatus> history =
        [
            ACollectReturnStatus()
                .WithLaeStab(registered)
                .WithReturnStatusCode(ReturnStatusCodes.Authorised)
                .WithUpdatedAt(LastRunDate.AddHours(1)),
            ACollectReturnStatus()
                .WithLaeStab(unregistered)
                .WithReturnStatusCode(ReturnStatusCodes.Authorised)
                .WithUpdatedAt(LastRunDate.AddHours(1))
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history);

        // Act
        var result = await store.GetWhatHasChangedAsync(LastRunDate);

        // Assert
        var change = result.Value.ShouldHaveSingleItem();
        change.LaeStab.ShouldBe(registered);
    }

    [Fact]
    public async Task When_getting_what_has_changed_it_should_only_return_changes_where_one_end_is_a_notifiable_status()
    {
        // Arrange
        var store = CreateLedgerStore([ReturnStatusCodes.Authorised, ReturnStatusCodes.Approved]);
        var notifiable = CreateTrackedLaeStab();
        var ignored = CreateTrackedLaeStab();
        await RegisterAsync(notifiable);
        await RegisterAsync(ignored);

        List<CollectReturnStatus> history =
        [
            // Rejected to Approved: the end is notifiable
            ACollectReturnStatus()
                .WithLaeStab(notifiable)
                .WithReturnStatusCode(ReturnStatusCodes.Rejected)
                .WithUpdatedAt(LastRunDate.AddDays(-1)),
            ACollectReturnStatus()
                .WithLaeStab(notifiable)
                .WithReturnStatusCode(ReturnStatusCodes.Approved)
                .WithUpdatedAt(LastRunDate.AddHours(1)),
            // Loaded_and_Validated to Rejected: neither end is notifiable
            ACollectReturnStatus()
                .WithLaeStab(ignored)
                .WithReturnStatusCode(ReturnStatusCodes.LoadedAndValidated)
                .WithUpdatedAt(LastRunDate.AddDays(-1)),
            ACollectReturnStatus()
                .WithLaeStab(ignored)
                .WithReturnStatusCode(ReturnStatusCodes.Rejected)
                .WithUpdatedAt(LastRunDate.AddHours(1))
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history);

        // Act
        var result = await store.GetWhatHasChangedAsync(LastRunDate);

        // Assert
        var change = result.Value.ShouldHaveSingleItem();
        change.LaeStab.ShouldBe(notifiable);
        change.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Approved);
    }

    [Fact]
    public async Task When_getting_what_has_changed_it_should_return_every_status_change_when_not_limited_to_notifiable_statuses()
    {
        // Arrange
        var store = CreateLedgerStore([ReturnStatusCodes.Authorised]);
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
                .WithReturnStatusCode(ReturnStatusCodes.Rejected)
                .WithUpdatedAt(LastRunDate.AddHours(1))
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history);

        // Act
        var result = await store.GetWhatHasChangedAsync(LastRunDate, limitToApprovedStatuses: false);

        // Assert
        var change = result.Value.ShouldHaveSingleItem();
        change.ReturnStatusCode.ShouldBe(ReturnStatusCodes.Rejected);
        change.PreviousReturnStatusCode.ShouldBe(ReturnStatusCodes.LoadedAndValidated);
    }

    [Fact]
    public async Task When_getting_what_has_changed_it_should_ignore_rows_from_another_collection()
    {
        // The ledger holds every collection, so without the Collection filter a school's rows from
        // a different census rank against each other and read as a status change.

        // Arrange
        var store = CreateLedgerStore();
        var laeStab = CreateTrackedLaeStab();
        await RegisterAsync(laeStab);

        List<CollectReturnStatus> history =
        [
            ACollectReturnStatus()
                .WithLaeStab(laeStab)
                .WithReturnStatusCode(ReturnStatusCodes.Authorised)
                .WithUpdatedAt(LastRunDate.AddHours(1))
                .WithCollection("SomeOtherCensus")
        ];

        await TestDatabaseHelper.InsertReturnStatusesAsync(history);

        // Act
        var result = await store.GetWhatHasChangedAsync(LastRunDate);

        // Assert
        result.Value.ShouldBeEmpty();
    }
}
