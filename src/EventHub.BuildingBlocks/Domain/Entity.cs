namespace BuildingBlocks.Domain;

/// <summary>
/// Base class for entities that can raise domain events.
/// </summary>
/// <typeparam name="TId">Entity identifier type.</typeparam>
public abstract class Entity<TId>
    where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Entity identifier.
    /// </summary>
    public TId Id { get; protected init; }

    /// <summary>
    /// Raised domain events.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
