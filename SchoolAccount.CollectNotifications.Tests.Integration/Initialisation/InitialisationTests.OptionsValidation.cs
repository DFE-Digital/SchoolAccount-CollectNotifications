using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Initialisation;

public partial class InitialisationTests
{
    [Fact]
    public async Task Should_throw_options_validation_exception_on_host_start_when_gov_notify_api_key_is_missing()
    {
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            ["GovNotify:ApiKey"] = "",
        });

        // Act & Assert
        var exception = await Should.ThrowAsync<OptionsValidationException>(async () => await host.StartAsync());

        exception.OptionsType.ShouldBe(typeof(GovNotifyOptions));
    }

    [Fact]
    public void ConfigureService_should_throw_when_azure_app_configuration_is_enabled_without_an_endpoint()
    {
        // Arrange, Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => CreateHost(new Dictionary<string, string?>
        {
            ["AzureAppConfiguration:Enabled"] = "true",
        }));

        exception.Message.ShouldContain("AzureAppConfiguration:Endpoint");
    }

    [Fact]
    public void ConfigureService_should_not_reach_for_azure_app_configuration_when_it_is_not_enabled()
    {
        // No endpoint configured and it builds fine, so nothing tried to connect.

        // Arrange, Act & Assert
        using var host = CreateHost();

        host.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("Census:JobName")]
    [InlineData("Census:Collection")]
    public async Task Should_throw_options_validation_exception_on_host_start_when_a_required_census_setting_is_missing(
        string settingKey)
    {
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            [settingKey] = "",
        });

        // Act & Assert
        var exception = await Should.ThrowAsync<OptionsValidationException>(async () => await host.StartAsync());

        exception.OptionsType.ShouldBe(typeof(CensusOptions));
    }

    [Fact]
    public async Task Should_throw_options_validation_exception_on_host_start_when_no_allowed_statuses_are_configured()
    {
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            ["Census:AllowedStatuses:0"] = null,
            ["Census:AllowedStatuses:1"] = null,
        });

        // Act & Assert
        var exception = await Should.ThrowAsync<OptionsValidationException>(async () => await host.StartAsync());

        exception.OptionsType.ShouldBe(typeof(CensusOptions));
    }

    [Fact]
    public void Should_bind_census_options_correctly_when_valid_values_are_provided()
    {
        const string testJobName = "collect-notifications";
        
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            ["Census:AllowedStatuses:0"] = "7",
            ["Census:AllowedStatuses:1"] = "10",
            ["Census:AllowedStatuses:2"] = "1",
            ["Census:JobName"] = testJobName,
        });

        // Act
        using var scope = host.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var censusOptions = sp.GetRequiredService<IOptions<CensusOptions>>().Value;

        // Assert
        censusOptions.ShouldSatisfyAllConditions(
            x => x.AllowedStatuses.ShouldBe([
                ReturnStatusCodes.Approved,
                ReturnStatusCodes.Authorised,
                ReturnStatusCodes.NoData,
            ]),
            x => x.JobName.ShouldBe(testJobName)
        );
    }
}
