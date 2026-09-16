namespace BuildingBlocks.Time;

/// <summary>
/// Default <see cref="IClockProvider"/> implementation that delegates to <see cref="TimeProvider.System"/>.
/// </summary>
/// <remarks>
/// The wrapper exists so DI registrations expose the EventHub-specific abstraction while still letting
/// callers depend on the framework <see cref="TimeProvider"/> directly when that is more convenient.
/// </remarks>
public sealed class SystemClockProvider : IClockProvider
{
    private readonly TimeProvider _timeProvider;

    public SystemClockProvider()
        : this(TimeProvider.System)
    {
    }

    /// <summary>
    /// Allows tests to inject a non-default <see cref="TimeProvider"/> while still using the production wrapper.
    /// </summary>
    public SystemClockProvider(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DateTimeOffset GetUtcNow() => _timeProvider.GetUtcNow();
}
