using Company.Template.Domain.Common;

namespace Company.Template.Application.Abstractions;

/// <summary>
/// Defines a contract for dispatching domain events to their respective handlers.
/// </summary>
/// <remarks>
/// This service centralizes event publication so handlers can react without aggregates 
/// depending on application or infrastructure services.
/// </remarks>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
