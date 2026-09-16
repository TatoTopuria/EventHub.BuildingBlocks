using BuildingBlocks.Abstractions.Observability;
using Serilog.Core;
using Serilog.Events;

namespace BuildingBlocks.Observability;

public sealed class CorrelationIdLogEventEnricher(ICorrelationContextAccessor correlationContextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = correlationContextAccessor.CorrelationId;
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return;
        }

        var property = propertyFactory.CreateProperty(LogFieldNames.CorrelationId, correlationId);
        logEvent.AddPropertyIfAbsent(property);
    }
}
