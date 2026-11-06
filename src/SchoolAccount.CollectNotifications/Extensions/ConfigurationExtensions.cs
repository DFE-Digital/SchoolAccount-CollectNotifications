using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace SchoolAccount.CollectNotifications.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddAzureAppConfiguration(
        this IConfigurationBuilder configurationBuilder,
        string endpoint
    )
    {
        var credentials = new DefaultAzureCredential();

        configurationBuilder.AddAzureAppConfiguration(options =>
            options
                .Connect(new Uri(endpoint), credentials)
                .ConfigureKeyVault(kv => kv.SetCredential(credentials))
        );

        return configurationBuilder;
    }
}
