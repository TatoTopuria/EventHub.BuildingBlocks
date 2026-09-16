using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Observability;

/// <summary>
/// Wires the shared EventHub OpenTelemetry tracing pipeline (F7). One call per service in
/// <c>Program.cs</c> after <see cref="SerilogHostBuilderExtensions.AddEventHubSerilog"/>.
/// </summary>
/// <remarks>
/// The instrumentation set is deliberately conservative — ASP.NET inbound, HTTP outbound, gRPC
/// outbound, and the MassTransit ActivitySource. EF Core spans are noisy by default and require
/// a contrib package; they're left off until a real ops case justifies the volume.
///
/// Export pipeline:
///   • Console exporter is always on (cheap, gives an immediate signal in <c>docker logs</c>).
///   • OTLP exporter activates when <c>Observability:Otlp:Endpoint</c> is configured. F7 does
///     not ship a collector container; a real deployment wires this at a Tempo / Jaeger /
///     New Relic / Honeycomb endpoint of its choosing.
/// </remarks>
public static class EventHubTracingExtensions
{
    public static WebApplicationBuilder AddEventHubTracing(this WebApplicationBuilder builder)
    {
        var serviceName = builder.Environment.ApplicationName;
        var environment = builder.Environment.EnvironmentName;
        var otlpEndpoint = builder.Configuration.GetValue<string>("Observability:Otlp:Endpoint");

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resourceBuilder => resourceBuilder
                .AddService(serviceName: serviceName, serviceVersion: ResolveAssemblyVersion())
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment", environment),
                    new KeyValuePair<string, object>("eventhub.service.name", serviceName)
                }))
            .WithTracing(tracerBuilder =>
            {
                tracerBuilder
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Don't trace the health / readiness probes — they're scraped every few
                        // seconds and would drown out the booking traces we actually care about.
                        options.Filter = httpContext =>
                            !httpContext.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation()
                    .AddGrpcClientInstrumentation()
                    // MassTransit emits its own ActivitySource named "MassTransit"; opting in
                    // here propagates trace context across the saga → outbox → consumer hops
                    // without bespoke header juggling. See DECISIONS.md "Observability v2".
                    .AddSource("MassTransit")
                    .AddConsoleExporter();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracerBuilder.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            });

        return builder;
    }

    private static string ResolveAssemblyVersion()
    {
        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
        return entryAssembly?.GetName().Version?.ToString() ?? "0.0.0";
    }
}
