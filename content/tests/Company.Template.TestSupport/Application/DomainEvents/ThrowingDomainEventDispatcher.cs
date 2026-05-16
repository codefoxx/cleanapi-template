using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Domain.Common;

namespace Company.Template.TestSupport.Application.DomainEvents;

public sealed class ThrowingDomainEventDispatcher : IDomainEventDispatcher
{
    public const string ExceptionMessage = "Domain event dispatch failed.";

    public Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(ExceptionMessage);
    }
}
