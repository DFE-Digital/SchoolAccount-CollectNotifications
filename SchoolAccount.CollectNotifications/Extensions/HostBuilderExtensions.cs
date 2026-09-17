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
    
            services.AddOptions<CensusOptions>()
                .Bind(hostContext.Configuration.GetSection(CensusOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
    
            services.AddEnrollmentStores(hostContext.Configuration);
            services.AddAzureBlobStorage(hostContext.Configuration);
    
            services.AddDatabase<LedgerDatabase>(hostContext.Configuration);
            services.AddSingleton<ILedgerStore, LedgerStore>();
    
            services.AddSingleton<IThreadingService, ThreadingService>();
            services.AddSingleton<ILastRanService, LastRanService>();
            services.AddSingleton<IGovNotifyService, GovNotifyService>();
            services.AddSingleton<StatusChangedLegerMonitoringService>();
        });

        return builder;
    }
}
