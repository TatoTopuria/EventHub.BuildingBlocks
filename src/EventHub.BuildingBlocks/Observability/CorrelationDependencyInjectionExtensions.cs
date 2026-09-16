using BuildingBlocks.Abstractions.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Observability;

public static class CorrelationDependencyInjectionExtensions
{
    public static IServiceCollection AddCorrelationContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        return services;
    }

    public static IApplicationBuilder UseCorrelationContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationContextMiddleware>();
    }
}
