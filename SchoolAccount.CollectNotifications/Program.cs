using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Services;

// Content root follows the binary rather than whatever directory this was launched from,
// otherwise appsettings.json is only found when the working directory happens to be the output
// one, and the app quietly behaves differently locally than it does in the container.
using var app = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
}).Configure().Build();

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
