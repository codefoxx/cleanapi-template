using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;

namespace Company.Template.Application.Tests.TestSupport;

/// <summary>
///     Captures dispatched domain events so application tests can assert observable side effects.
/// </summary>
/// <remarks>
///     This test double keeps domain-event assertions explicit without coupling tests to logging,
///     infrastructure handlers, or a mocking framework. It is intended for application-level tests
///     that need to verify which events were published after persistence.
/// </remarks>
public sealed class RecordingDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly List<IDomainEvent> _dispatchedEvents = [];

    public IReadOnlyList<IDomainEvent> DispatchedEvents => _dispatchedEvents;

    public Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        _dispatchedEvents.AddRange(domainEvents);

        return Task.CompletedTask;
    }
}
