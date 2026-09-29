using System.Reflection;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SchoolAccount.CollectNotifications.Extensions;

public static class MonitoringExtensions
{
    internal const string ServiceName = "schoolaccount-collectnotify-app";
    internal const string AppInsightsKey = "APPLICATIONINSIGHTS_CONNECTION_STRING";
    internal const string AppInsightsVersionedKey = "APPLICATIONINSIGHTS_INCLUDE_VERSION";
    internal const string OtlpEndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public static HostApplicationBuilder AddMonitoring(this HostApplicationBuilder builder)
    {
        var appInsightsConnectionString = builder.Configuration[AppInsightsKey];
        var otlpEndpoint = builder.Configuration[OtlpEndpointKey];

        var hasAppInsights = !string.IsNullOrWhiteSpace(appInsightsConnectionString);
        var hasOtlp = !string.IsNullOrWhiteSpace(otlpEndpoint);

        if (!hasAppInsights && !hasOtlp)
        {
            return builder;
        }

        var version = GetServiceVersion();
        var serviceName = ServiceName;
        if (
            builder.Configuration.GetValue<bool>(AppInsightsVersionedKey)
            && !string.IsNullOrWhiteSpace(version)
        )
        {
            serviceName += ":" + version.Split('+')[0];
        }

        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: version);

        builder
            .Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(opt => opt.RecordException = true);

                if (hasAppInsights)
                {
                    tracing.AddAzureMonitorTraceExporter(o =>
                    {
                        o.ConnectionString = appInsightsConnectionString;
                        o.Credential = new DefaultAzureCredential();
                    });
                }

                if (hasOtlp)
                {
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint!));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(resourceBuilder).AddHttpClientInstrumentation();

                if (hasAppInsights)
                {
                    metrics.AddAzureMonitorMetricExporter(o =>
                    {
                        o.ConnectionString = appInsightsConnectionString;
                        o.Credential = new DefaultAzureCredential();
                    });
                }

                if (hasOtlp)
                {
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint!));
                }
            });

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.SetResourceBuilder(resourceBuilder);
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;

            if (hasAppInsights)
            {
                logging.AddAzureMonitorLogExporter(o =>
                {
                    o.ConnectionString = appInsightsConnectionString;
                    o.Credential = new DefaultAzureCredential();
                });
            }

            if (hasOtlp)
            {
                logging.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint!));
            }
        });

        return builder;
    }

    internal static string GetServiceVersion()
    {
        var assembly = typeof(MonitoringExtensions).Assembly;
        return assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? string.Empty;
    }
}
