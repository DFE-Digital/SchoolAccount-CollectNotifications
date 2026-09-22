using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models.Databases;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services;
using SchoolAccount.CollectNotifications.Stores;

namespace SchoolAccount.CollectNotifications.Extensions;

public static class HostBuilderExtensions
{
    private const string AppConfigurationEnabledKey = "AzureAppConfiguration:Enabled";
    private const string AppConfigurationEndpointKey = "AzureAppConfiguration:Endpoint";

    public static HostApplicationBuilder Configure(this HostApplicationBuilder builder)
    {
        var useAppConfiguration = builder.Configuration.GetValue<bool>(AppConfigurationEnabledKey);

        if (useAppConfiguration)
        {
            var endpoint = builder.Configuration[AppConfigurationEndpointKey];

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException($"The setting `{AppConfigurationEndpointKey}` was not found.");
            }

            builder.Configuration.AddAzureAppConfiguration(endpoint);
        }

        builder.AddValidatedOptions<GovNotifyOptions>(GovNotifyOptions.SectionName);
        builder.AddValidatedOptions<CensusOptions>(CensusOptions.SectionName);

        builder.Services.AddDatabase<LedgerDatabase>(builder.Configuration);
        builder.Services.AddSingleton<ILedgerStore, LedgerStore>();

        builder.Services.AddSingleton<ILastRanService, LastRanService>();
        builder.Services.AddSingleton<IGovNotifyService, GovNotifyService>();
        builder.Services.AddSingleton<StatusChangedLedgerMonitoringServiceInstrumentation>();
        builder.Services.AddSingleton<StatusChangedLedgerMonitoringService>();

        return builder;
    }

    private static void AddValidatedOptions<TOptions>(this HostApplicationBuilder builder, string sectionName)
        where TOptions : class
    {
        builder.Services.AddOptions<TOptions>()
            .Bind(builder.Configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
