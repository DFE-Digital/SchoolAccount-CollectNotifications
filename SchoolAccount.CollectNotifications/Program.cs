using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Services;

using var app = Host.CreateDefaultBuilder(args).Configure().Build();

// Starting the host is what runs the options validation registered by ValidateOnStart. Building it
// alone doesn't, so without this a missing setting surfaces partway through a run, when something
// first reads IOptions<T>.Value, rather than before we touch the database.
await app.StartAsync();

try
{
    // Stopping the host signals this, so a SIGTERM from the container runtime cancels the run
    // rather than being ignored until the job is killed.
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

    await app.Services
        .GetRequiredService<StatusChangedLedgerMonitoringService>()
        .InvokeAsync(lifetime.ApplicationStopping);
}
finally
{
    await app.StopAsync();
}
