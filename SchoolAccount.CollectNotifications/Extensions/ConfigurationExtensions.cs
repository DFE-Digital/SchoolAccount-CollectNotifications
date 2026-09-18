using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace SchoolAccount.CollectNotifications.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddAzureAppConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        var configuration = configurationBuilder.Build();
        var endpoint =
            configuration["AzureAppConfiguration:Endpoint"]
            ?? throw new InvalidOperationException(
                "The setting `AzureAppConfiguration:Endpoint` was not found."
            );

        var credentials = new DefaultAzureCredential();

        configurationBuilder.AddAzureAppConfiguration(options =>
            options
                .Connect(new Uri(endpoint), credentials)
                .ConfigureKeyVault(kv => kv.SetCredential(credentials))
        );

        return configurationBuilder;
    }
}