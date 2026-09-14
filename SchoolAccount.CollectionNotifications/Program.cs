using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectionNotifications.Extensions;
using SchoolAccount.CollectionNotifications.Models.Databases;
using SchoolAccount.CollectionNotifications.Services;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    services.AddEnrollmentStores(hostContext.Configuration);
    services.AddDatabase<LedgerDatabase>(hostContext.Configuration);
    services.AddTransient<StatusChangedLegerMonitoringService>();
});

var app = builder.Build();
using var scope = app.Services.CreateScope();

var monitoringService = scope.ServiceProvider.GetRequiredService<StatusChangedLegerMonitoringService>();
await monitoringService.InvokeAsync();