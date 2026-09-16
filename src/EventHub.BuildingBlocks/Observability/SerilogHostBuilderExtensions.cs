using BuildingBlocks.Abstractions.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace BuildingBlocks.Observability;

public static class SerilogHostBuilderExtensions
{
    public static WebApplicationBuilder AddEventHubSerilog(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CorrelationIdLogEventEnricher>();
        builder.Services.AddSingleton<UserIdLogEventEnricher>();

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            var elasticUri = context.Configuration.GetValue<string>("Observability:Elasticsearch:Uri");

            var correlationEnricher = services.GetRequiredService<CorrelationIdLogEventEnricher>();
            var userIdEnricher = services.GetRequiredService<UserIdLogEventEnricher>();

            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty(LogFieldNames.ServiceName, context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty(LogFieldNames.Environment, context.HostingEnvironment.EnvironmentName)
                .Enrich.With(correlationEnricher)
                .Enrich.With(userIdEnricher)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(elasticUri))
            {
                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = BuildIndexFormat(context.HostingEnvironment.ApplicationName)
                });
            }
        });

        return builder;
    }

    private static string BuildIndexFormat(string serviceName)
    {
        var normalizedService = serviceName
            .ToLowerInvariant()
            .Replace('.', '-')
            .Replace('_', '-');

        return $"eventhub-logs-{normalizedService}-{{0:yyyy.MM}}";
    }
}
