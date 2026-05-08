using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Company.Template.Infrastructure.DomainEvents;

internal sealed class LoggingDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<LoggingDomainEventDispatcher> _logger;

    public LoggingDomainEventDispatcher(ILogger<LoggingDomainEventDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("Domain event dispatched: {DomainEvent}", domainEvent.GetType().Name);
        }

        return Task.CompletedTask;
    }
}
