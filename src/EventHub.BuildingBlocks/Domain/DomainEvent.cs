namespace BuildingBlocks.Domain;

/// <summary>
/// Represents a domain event raised by an aggregate.
/// </summary>
public abstract record DomainEvent(DateTime OccurredOnUtc);
