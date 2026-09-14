using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectionNotifications.Extensions;
using SchoolAccount.CollectionNotifications.Services;

var app = Host.CreateDefaultBuilder(args).ConfigureService().Build();
using var scope = app.Services.CreateScope();
await scope.ServiceProvider
    .GetRequiredService<StatusChangedLegerMonitoringService>()
    .InvokeAsync();