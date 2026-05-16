using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Domain.Common;

namespace Company.Template.TestSupport.Application.DomainEvents;

public sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
