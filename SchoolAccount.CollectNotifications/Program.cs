using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Services;

var app = Host.CreateDefaultBuilder(args).Configure().Build();
using var scope = app.Services.CreateScope();
await scope.ServiceProvider
    .GetRequiredService<StatusChangedLedgerMonitoringService>()
    .InvokeAsync();