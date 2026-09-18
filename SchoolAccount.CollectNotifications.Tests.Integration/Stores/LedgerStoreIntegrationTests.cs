using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Databases;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Stores;
using SchoolAccount.CollectNotifications.Tests.Integration.Helpers;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Stores;

public partial class LedgerStoreIntegrationTests : IAsyncLifetime
{
    private readonly List<string> _createdLaeStabs = [];
    private readonly DbConnectionFactory<LedgerDatabase> _connectionFactory = new(TestDatabaseHelper.ConnectionString);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _connectionFactory.DisposeAsync();
        if (_createdLaeStabs.Count > 0)
        {
            await TestDatabaseHelper.DeleteReturnStatusesByLaeStabAsync(_createdLaeStabs);
        }
    }

    private string CreateTrackedLaeStab()
    {
        var laeStab = LaeStabHelpers.GenerateUnique();
        _createdLaeStabs.Add(laeStab);
        return laeStab;
    }

    private LedgerStore CreateLedgerStore(List<ReturnStatusCodes>? allowedStatuses = null)
    {
        var censusOptions = Options.Create(new CensusOptions
        {
            AllowedStatuses = allowedStatuses ?? [ReturnStatusCodes.Authorised, ReturnStatusCodes.Approved]
        });

        return new LedgerStore(_connectionFactory, censusOptions);
    }
}
