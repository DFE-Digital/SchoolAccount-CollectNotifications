using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectNotifications.Extensions;

namespace SchoolAccount.CollectNotifications.IntegrationTests.Initialisation;

public partial class InitialisationTests
{
    private const string ValidDummyGovNotifyApiKey =
        "test_key-00000000-0000-0000-0000-000000000000-00000000-0000-0000-0000-000000000000";

    private const string LedgerConnectionString =
        "Server=localhost;Database=CollectStateLedger;User Id=sa;Password=test;TrustServerCertificate=true";

    private const string CensusJobName = "collect-notifications";
    private const string CensusCollection = "SchoolCensus2025_Spring";

    private static readonly Dictionary<string, string?> ValidBaseConfiguration = new()
    {
        ["GovNotify:ApiKey"] = ValidDummyGovNotifyApiKey,
        ["Census:ConnectionString"] = LedgerConnectionString,
        ["Census:JobName"] = CensusJobName,
        ["Census:Collection"] = CensusCollection,
        ["Census:AllowedStatuses:0"] = "7",
        ["Census:AllowedStatuses:1"] = "10",
    };

    /// <summary>
    /// Builds a host on the valid base configuration, with overrides applied on top. A null
    /// override removes the key rather than setting it to null, which is the only way to express
    /// an absent list entry: leaving Census:AllowedStatuses:0 present but null still binds an
    /// element, so the list never comes out empty.
    /// </summary>
    private static IHost CreateHost(Dictionary<string, string?>? configurationOverrides = null)
    {
        var config = new Dictionary<string, string?>(ValidBaseConfiguration);
        foreach (var kvp in configurationOverrides ?? [])
        {
            if (kvp.Value is null)
            {
                config.Remove(kvp.Key);
                continue;
            }

            config[kvp.Key] = kvp.Value;
        }

        // DisableDefaults keeps this hermetic. Without it the builder picks up environment
        // variables and appsettings from wherever the tests happen to be running.
        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings
            {
                EnvironmentName = "IntegrationTest",
                DisableDefaults = true,
            }
        );

        builder.Configuration.AddInMemoryCollection(config);

        return builder.Configure().Build();
    }
}
