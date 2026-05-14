namespace Company.Template.Domain.Common;

/// <summary>
///     Base class for all Aggregate Roots in the domain.
/// </summary>
/// <remarks>
///     An aggregate root is a domain entity that maintains consistency within its boundary
///     by protecting domain rules and invariants. It acts as the primary entry point for
///     modifying the state of the aggregate and is responsible for collecting domain events
///     that describe facts that happened.
/// </remarks>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    ///     Records a domain event that occurred within this aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to record.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    ///     Removes all in-memory domain events from the aggregate.
    /// </summary>
    /// <remarks>
    ///     This is typically called by the application infrastructure after the events have been
    ///     collected for dispatching or persistence, for example after they have been written to
    ///     an outbox. The aggregate does not track per-event delivery state.
    /// </remarks>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
