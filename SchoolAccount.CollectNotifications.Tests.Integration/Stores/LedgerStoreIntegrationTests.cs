using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Stores;
using SchoolAccount.CollectNotifications.Tests.Integration.Helpers;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Stores;

public partial class LedgerStoreIntegrationTests : IAsyncLifetime
{
    private const string TestCollection = "Census";
    private const string DefaultEmail = "head@school.sch.uk";

    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly List<string> _createdLaeStabs = [];
    private readonly DbConnectionFactory _connectionFactory = new(TestDatabaseHelper.ConnectionString);

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        if (_createdLaeStabs.Count > 0)
        {
            await TestDatabaseHelper.DeleteReturnStatusesByLaeStabAsync(_createdLaeStabs);
            await TestDatabaseHelper.DeleteRegisteredUsersByLaeStabAsync(_createdLaeStabs);
        }
    }

    private string CreateTrackedLaeStab()
    {
        var laeStab = LaeStabHelpers.GenerateUnique();
        _createdLaeStabs.Add(laeStab);
        return laeStab;
    }

    /// <summary>
    /// The query joins RegisteredUsers, so a school with nobody registered never comes back.
    /// Every test that expects results needs at least one recipient.
    /// </summary>
    private static async Task RegisterAsync(string laeStab, params string[] emails)
    {
        await TestDatabaseHelper.InsertRegisteredUsersAsync(
            laeStab,
            emails.Length > 0 ? emails : [DefaultEmail]);
    }

    private LedgerStore CreateLedgerStore(List<ReturnStatusCodes>? allowedStatuses = null)
    {
        var censusOptions = Options.Create(new CensusOptions
        {
            AllowedStatuses = allowedStatuses ?? [ReturnStatusCodes.Authorised, ReturnStatusCodes.Approved],
            Collection = TestCollection,
        });

        return new LedgerStore(_connectionFactory, censusOptions);
    }
}
