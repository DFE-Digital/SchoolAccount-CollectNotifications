using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectionNotifications.Models.Databases;
using SchoolAccount.CollectionNotifications.Models.Options;
using SchoolAccount.CollectionNotifications.Services;
using SchoolAccount.CollectionNotifications.Stores;

namespace SchoolAccount.CollectionNotifications.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureService(this IHostBuilder builder)
    {
        builder.ConfigureServices((hostContext, services) =>
        {
            services.AddOptions<GovNotifyOptions>()
                .Bind(hostContext.Configuration.GetSection(GovNotifyOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
    
            services.AddOptions<ThreadingOptions>()
                .Bind(hostContext.Configuration.GetSection(ThreadingOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
    
            services.AddEnrollmentStores(hostContext.Configuration);
    
            services.AddDatabase<LedgerDatabase>(hostContext.Configuration);
            services.AddSingleton<LedgerStore>();
    
            services.AddSingleton<LastRanService>();
            services.AddSingleton<GovNotifyService>();
            services.AddSingleton<StatusChangedLegerMonitoringService>();
        });

        return builder;
    }
}