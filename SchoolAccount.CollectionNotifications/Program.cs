using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectionNotifications.Extensions;
using SchoolAccount.CollectionNotifications.Models.Databases;
using SchoolAccount.CollectionNotifications.Models.Options;
using SchoolAccount.CollectionNotifications.Services;
using SchoolAccount.CollectionNotifications.Stores;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    services.AddOptions<GovNotifyOptions>()
        .Bind(hostContext.Configuration.GetSection(GovNotifyOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();
    
    services.AddEnrollmentStores(hostContext.Configuration);
    
    services.AddDatabase<LedgerDatabase>(hostContext.Configuration);
    services.AddSingleton<LedgerStore>();
    
    services.AddSingleton<LastRanService>();
    services.AddSingleton<GovNotifyService>();
    services.AddSingleton<StatusChangedLegerMonitoringService>();
});

var app = builder.Build();
using var scope = app.Services.CreateScope();

var monitoringService = scope.ServiceProvider.GetRequiredService<StatusChangedLegerMonitoringService>();
await monitoringService.InvokeAsync();