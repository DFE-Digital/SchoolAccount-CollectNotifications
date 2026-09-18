using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectNotifications.Extensions;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Initialisation;

public partial class InitialisationTests
{
    private const string ValidDummyGovNotifyApiKey =
        "test_key-00000000-0000-0000-0000-000000000000-00000000-0000-0000-0000-000000000000";

    private const string LedgerConnectionString =
        "Server=localhost;Database=CollectStateLedger;User Id=sa;Password=test;TrustServerCertificate=true";

    private const string EnrollmentCsvFilePath = "/path/to/recipients.csv";
    private const string ThreadingMaxParallelism = "5";
    private const string ThreadingBatchAmount = "25";
    private const string ThreadingBatchWait = "2";
    private const string ThreadingItemWait = "0";

    private static readonly Dictionary<string, string?> ValidBaseConfiguration = new()
    {
        ["ConnectionStrings:LedgerDatabase"] = LedgerConnectionString,
        ["GovNotify:ApiKey"] = ValidDummyGovNotifyApiKey,
        ["Enrollment:Csv:FilePath"] = EnrollmentCsvFilePath,
        ["Threading:MaxDegreeOfParallelism"] = ThreadingMaxParallelism,
        ["Threading:BatchAmount"] = ThreadingBatchAmount,
        ["Threading:BatchWaitAmountInSec"] = ThreadingBatchWait,
        ["Threading:ItemWaitAmountInSec"] = ThreadingItemWait,
        ["Census:AllowedStatuses:0"] = "7",
        ["Census:AllowedStatuses:1"] = "10"
    };

    private static IHost CreateHost(Dictionary<string, string?>? configurationOverrides = null)
    {
        var config = new Dictionary<string, string?>(ValidBaseConfiguration);
        foreach (var kvp in configurationOverrides ?? [])
        {
            config[kvp.Key] = kvp.Value;
        }

        return new HostBuilder()
            .UseEnvironment("IntegrationTest")
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(config))
            .Configure()
            .Build();
    }
}
