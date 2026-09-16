using BuildingBlocks.Domain;

namespace BuildingBlocks.Abstractions;

/// <summary>
/// Dispatches domain events raised by aggregates.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches the provided domain events.
    /// </summary>
    Task DispatchAsync(IReadOnlyCollection<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
