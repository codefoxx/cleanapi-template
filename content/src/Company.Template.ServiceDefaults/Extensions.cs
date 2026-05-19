using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>
///     Provides shared Aspire service defaults for template-hosted applications.
/// </summary>
/// <remarks>
///     The defaults centralize service discovery, HTTP client resilience, OpenTelemetry, and development health endpoints
///     so
///     service projects get consistent operational behavior without duplicating hosting setup.
/// </remarks>
public static class Extensions
{
    private const string AlivenessEndpointPath = "/alive";
    private const string HealthEndpointPath = "/health";
    private const string OtlpExporterEndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks(HealthEndpointPath);

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks(AlivenessEndpointPath,
            new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live")
            });

        return app;
    }

    private static bool IsHealthCheckRequest(PathString path)
    {
        return path.StartsWithSegments(
                HealthEndpointPath,
                StringComparison.OrdinalIgnoreCase)
         || path.StartsWithSegments(
                AlivenessEndpointPath,
                StringComparison.OrdinalIgnoreCase);
    }
    // Uncomment the following lines to enable the Azure Monitor exporter.
    // private const string ApplicationInsightsConnectionStringKey = "APPLICATIONINSIGHTS_CONNECTION_STRING";

    extension<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        public TBuilder AddServiceDefaults()
        {
            builder.ConfigureOpenTelemetry();

            builder.AddDefaultHealthChecks();

            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                // Turn on resilience by default
                http.AddStandardResilienceHandler();

                // Turn on service discovery by default
                http.AddServiceDiscovery();
            });

            // Uncomment the following to restrict the allowed schemes for service discovery.
            // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
            // {
            //     options.AllowedSchemes = ["https"];
            // });

            return builder;
        }

        private TBuilder ConfigureOpenTelemetry()
        {
            ServiceDefaultsOpenTelemetryOptions openTelemetry = builder.Configuration
                                                                       .GetSection(ServiceDefaultsOpenTelemetryOptions.SectionName)
                                                                       .Get<ServiceDefaultsOpenTelemetryOptions>() ?? new ServiceDefaultsOpenTelemetryOptions();

            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.Services.AddOpenTelemetry()
                   .WithMetrics(metrics =>
                    {
                        metrics
                           .AddMeter(openTelemetry.Meters)
                           .AddAspNetCoreInstrumentation()
                           .AddHttpClientInstrumentation()
                           .AddRuntimeInstrumentation();
                    })
                   .WithTracing(tracing =>
                    {
                        tracing
                           .AddSource(builder.Environment.ApplicationName)
                           .AddSource(openTelemetry.Sources)
                           .AddAspNetCoreInstrumentation(options =>
                            {
                                // Exclude health check requests from tracing
                                options.Filter = context => !IsHealthCheckRequest(context.Request.Path);
                            })
                            // Uncomment the following line to enable gRPC instrumentation.
                            // Requires the OpenTelemetry.Instrumentation.GrpcNetClient package.
                            // .AddGrpcClientInstrumentation()
                           .AddHttpClientInstrumentation();
                    });

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        private TBuilder AddDefaultHealthChecks()
        {
            builder.Services.AddHealthChecks()
                    // Add a default liveness check to ensure app is responsive
                   .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            return builder;
        }

        private TBuilder AddOpenTelemetryExporters()
        {
            bool useOtlpExporter = !string.IsNullOrWhiteSpace(
                builder.Configuration[OtlpExporterEndpointKey]);

            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }

            // Uncomment the following lines to enable the Azure Monitor exporter.
            // Requires the Azure.Monitor.OpenTelemetry.AspNetCore package.
            // if (!string.IsNullOrEmpty(builder.Configuration[ApplicationInsightsConnectionStringKey]))
            // {
            //     builder.Services.AddOpenTelemetry()
            //        .UseAzureMonitor();
            // }

            return builder;
        }
    }
}

internal sealed class ServiceDefaultsOpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";

    public string[] Meters { get; init; } = [];

    public string[] Sources { get; init; } = [];
}
