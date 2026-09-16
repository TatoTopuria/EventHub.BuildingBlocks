using System.Security.Claims;
using BuildingBlocks.Abstractions.Observability;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace BuildingBlocks.Observability;

public sealed class UserIdLogEventEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
            ?? CorrelationConstants.UnknownUserId;

        var property = propertyFactory.CreateProperty(LogFieldNames.UserId, userId);
        logEvent.AddPropertyIfAbsent(property);
    }
}
