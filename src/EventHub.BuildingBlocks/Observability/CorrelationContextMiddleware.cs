using System.Security.Claims;
using BuildingBlocks.Abstractions.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Observability;

public sealed class CorrelationContextMiddleware(
    RequestDelegate next,
    ICorrelationContextAccessor correlationContextAccessor,
    IHostEnvironment hostEnvironment,
    ILogger<CorrelationContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Request.Headers[CorrelationConstants.HeaderName] = correlationId;
        context.Items[CorrelationConstants.ItemName] = correlationId;
        context.TraceIdentifier = correlationId;
        correlationContextAccessor.CorrelationId = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationConstants.HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value
            ?? CorrelationConstants.UnknownUserId;

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            [LogFieldNames.CorrelationId] = correlationId,
            [LogFieldNames.ServiceName] = hostEnvironment.ApplicationName,
            [LogFieldNames.UserId] = userId,
            [LogFieldNames.Environment] = hostEnvironment.EnvironmentName
        }))
        {
            try
            {
                await next(context);
            }
            finally
            {
                correlationContextAccessor.CorrelationId = null;
            }
        }
    }
}
