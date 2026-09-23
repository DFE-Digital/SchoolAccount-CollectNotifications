using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Services;
using SchoolAccount.CollectNotifications.Stores;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Initialisation;

public partial class InitialisationTests
{
    [Fact]
    public void ConfigureService_should_resolve_all_core_services_when_default_valid_configuration_is_provided()
    {
        // Arrange
        using var host = CreateHost();

        // Act & Assert
        using var scope = host.Services.CreateScope();
        var sp = scope.ServiceProvider;

        sp.GetRequiredService<StatusChangedLedgerMonitoringService>().ShouldNotBeNull();
        sp.GetRequiredService<ILedgerStore>().ShouldBeOfType<LedgerStore>();
        sp.GetRequiredService<ILastRanService>().ShouldBeOfType<LastRanService>();
        sp.GetRequiredService<IGovNotifyService>().ShouldBeOfType<GovNotifyService>();
        sp.GetRequiredService<IDbConnectionFactory>().ShouldNotBeNull();
    }
}
