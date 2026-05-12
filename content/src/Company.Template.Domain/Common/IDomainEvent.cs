namespace Company.Template.Domain.Common;

/// <summary>
/// Defines a domain event that represents a significant occurrence in the business domain.
/// </summary>
/// <remarks>
/// Domain events describe facts that have happened in the domain. They are used to
/// trigger side effects through event handlers, ensuring that the primary aggregate
/// remains focused on its own invariants without being coupled to the resulting
/// reactions in other parts of the system.
/// </remarks>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
