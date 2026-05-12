using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;

namespace Company.Template.Infrastructure.DomainEvents;

/// <summary>
/// Infrastructure domain-event dispatcher that records observed events to logs.
/// </summary>
/// <remarks>
/// This implementation represents a composition placeholder: aggregates only record facts, and dispatching happens
/// after persistence through the application abstraction without feeding reactions back into the aggregate.
/// </remarks>
internal sealed class LoggingDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<LoggingDomainEventDispatcher> _logger;

    public LoggingDomainEventDispatcher(ILogger<LoggingDomainEventDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            _logger.LogInformation("Domain event dispatched: {DomainEvent}", domainEvent.GetType().Name);
        }

        return Task.CompletedTask;
    }
}
