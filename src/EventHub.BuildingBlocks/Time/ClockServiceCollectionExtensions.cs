using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Time;

/// <summary>
/// Registers the EventHub clock abstraction.
/// </summary>
public static class ClockServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IClockProvider"/> (default: <see cref="SystemClockProvider"/>) and
    /// <see cref="TimeProvider"/> (default: <see cref="TimeProvider.System"/>) as singletons.
    /// </summary>
    /// <remarks>
    /// Both registrations are present so callers that already take <see cref="TimeProvider"/> keep
    /// working, while new code should prefer <see cref="IClockProvider"/>.
    /// </remarks>
    public static IServiceCollection AddEventHubClock(this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IClockProvider, SystemClockProvider>();
        return services;
    }
}
