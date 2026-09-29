using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;
using SchoolAccount.CollectNotifications.Extensions;

namespace SchoolAccount.CollectNotifications.UnitTests.Extensions;

public class MonitoringExtensionsTests
{
    private const string _otlpEndpoint = "http://localhost:4317";
    private const string _appInsightsConnection =
        "InstrumentationKey=00000000-0000-0000-0000-000000000000";

    [Fact]
    public void AddMonitoring_without_configuration_should_not_register_tracer_provider()
    {
        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings { DisableDefaults = true }
        );
        builder.AddMonitoring();
        using var host = builder.Build();

        var tracerProvider = host.Services.GetService<TracerProvider>();
        tracerProvider.ShouldBeNull();
    }

    [Fact]
    public void AddMonitoring_with_otlp_endpoint_should_register_tracer_provider()
    {
        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings { DisableDefaults = true }
        );
        builder.Configuration[MonitoringExtensions.OtlpEndpointKey] = _otlpEndpoint;
        builder.AddMonitoring();
        using var host = builder.Build();

        var tracerProvider = host.Services.GetService<TracerProvider>();
        tracerProvider.ShouldNotBeNull();
    }

    [Fact]
    public void AddMonitoring_with_app_insights_should_register_tracer_provider()
    {
        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings { DisableDefaults = true }
        );
        builder.Configuration[MonitoringExtensions.AppInsightsKey] = _appInsightsConnection;
        builder.AddMonitoring();
        using var host = builder.Build();

        var tracerProvider = host.Services.GetService<TracerProvider>();
        tracerProvider.ShouldNotBeNull();
    }

    [Fact]
    public void GetServiceVersion_should_return_non_empty_version()
    {
        var version = MonitoringExtensions.GetServiceVersion();
        version.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void AddMonitoring_should_set_service_name_and_service_version_on_resource()
    {
        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings { DisableDefaults = true }
        );
        builder.Configuration[MonitoringExtensions.OtlpEndpointKey] = _otlpEndpoint;
        builder.AddMonitoring();
        using var host = builder.Build();

        var tracerProvider = host.Services.GetService<TracerProvider>();
        tracerProvider.ShouldNotBeNull();

        var resource = tracerProvider.GetResource();
        resource.Attributes.ShouldContain(a =>
            a.Key == "service.name" && (string)a.Value == MonitoringExtensions.ServiceName
        );
        resource.Attributes.ShouldContain(a =>
            a.Key == "service.version" && !string.IsNullOrWhiteSpace((string)a.Value)
        );
    }

    [Fact]
    public void AddMonitoring_when_include_version_is_true_should_append_version_to_service_name()
    {
        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings { DisableDefaults = true }
        );
        builder.Configuration[MonitoringExtensions.OtlpEndpointKey] = _otlpEndpoint;
        builder.Configuration[MonitoringExtensions.AppInsightsVersionedKey] = "true";
        builder.AddMonitoring();
        using var host = builder.Build();

        var tracerProvider = host.Services.GetService<TracerProvider>();
        tracerProvider.ShouldNotBeNull();

        var expectedVersion = MonitoringExtensions.GetServiceVersion().Split('+')[0];
        var expectedServiceName = $"{MonitoringExtensions.ServiceName}:{expectedVersion}";

        var resource = tracerProvider.GetResource();
        resource.Attributes.ShouldContain(a =>
            a.Key == "service.name" && (string)a.Value == expectedServiceName
        );
    }
}
