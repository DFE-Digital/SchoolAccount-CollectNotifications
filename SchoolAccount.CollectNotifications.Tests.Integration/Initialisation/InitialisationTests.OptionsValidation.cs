using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Initialisation;

public partial class InitialisationTests
{
    [Fact]
    public async Task ConfigureService_should_throw_options_validation_exception_on_host_start_when_gov_notify_api_key_is_missing()
    {
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            ["GovNotify:ApiKey"] = ""
        });

        // Act & Assert
        var exception = await Should.ThrowAsync<OptionsValidationException>(async () =>
        {
            await host.StartAsync();
        });

        exception.OptionsType.ShouldBe(typeof(GovNotifyOptions));
    }

    [Fact]
    public async Task ConfigureService_should_throw_options_validation_exception_on_host_start_when_gov_notify_from_address_is_invalid()
    {
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            ["GovNotify:FromAddress"] = "not-a-valid-email"
        });

        // Act & Assert
        var exception = await Should.ThrowAsync<OptionsValidationException>(async () =>
        {
            await host.StartAsync();
        });

        exception.OptionsType.ShouldBe(typeof(GovNotifyOptions));
    }

    [Fact]
    public void ConfigureService_should_throw_argument_exception_during_host_build_when_ledger_database_connection_string_is_missing()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentException>(() =>
        {
            new HostBuilder()
                .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["GovNotify:ApiKey"] = ValidDummyGovNotifyApiKey,
                    ["Enrollment:Csv:FilePath"] = "/path.csv"
                }))
                .ConfigureService()
                .Build();
        });
    }

    [Fact]
    public void ConfigureService_should_bind_threading_and_census_options_correctly_when_valid_values_are_provided()
    {
        const int testThreadingBatchAmount = 100;
        const int testThreadingBatchWaitAmountInSec = 5;
        const int testThreadingItemWaitAmountInSec = 2;
        const string testLastRunBlobName = "schoolaccount/collect/lastran.json";
        
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            ["Threading:BatchAmount"] = testThreadingBatchAmount.ToString(),
            ["Threading:BatchWaitAmountInSec"] = testThreadingBatchWaitAmountInSec.ToString(),
            ["Threading:ItemWaitAmountInSec"] = testThreadingItemWaitAmountInSec.ToString(),
            ["Census:AllowedStatuses:0"] = "7",
            ["Census:AllowedStatuses:1"] = "10",
            ["Census:AllowedStatuses:2"] = "1",
            ["Census:LastRunBlobName"] = testLastRunBlobName
        });

        // Act
        using var scope = host.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var threadingOptions = sp.GetRequiredService<IOptions<ThreadingOptions>>().Value;
        var censusOptions = sp.GetRequiredService<IOptions<CensusOptions>>().Value;

        // Assert
        threadingOptions.ShouldSatisfyAllConditions(
            x => x.BatchAmount.ShouldBe(testThreadingBatchAmount),
            x => x.BatchWaitAmountInSec.ShouldBe(testThreadingBatchWaitAmountInSec),
            x => x.ItemWaitAmountInSec.ShouldBe(testThreadingItemWaitAmountInSec)
        );

        censusOptions.ShouldSatisfyAllConditions(
            x => x.AllowedStatuses.ShouldBe([
                ReturnStatusCodes.Approved,
                ReturnStatusCodes.Authorised,
                ReturnStatusCodes.NoData
            ]),
            x => x.LastRunBlobName.ShouldBe(testLastRunBlobName)
        );
    }
}
