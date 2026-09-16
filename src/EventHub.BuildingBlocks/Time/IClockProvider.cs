namespace BuildingBlocks.Time;

/// <summary>
/// Single source of truth for current time across the EventHub platform.
/// </summary>
/// <remarks>
/// Modeled after the .NET 8 <see cref="System.TimeProvider"/> API surface so callers may swap from
/// one to the other without changing call sites. Use this abstraction instead of <c>DateTime.UtcNow</c>
/// directly so that time-dependent behavior is deterministic in tests.
/// </remarks>
public interface IClockProvider
{
    /// <summary>
    /// Returns the current Coordinated Universal Time (UTC) as a <see cref="DateTimeOffset"/>.
    /// </summary>
    DateTimeOffset GetUtcNow();
}
