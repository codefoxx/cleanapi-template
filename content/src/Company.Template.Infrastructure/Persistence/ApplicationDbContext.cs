using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Application.Abstractions.Persistence;
using Company.Template.Domain.Common;

namespace Company.Template.Infrastructure.Persistence;

/// <summary>
///     EF Core unit-of-work implementation for the template application.
/// </summary>
/// <remarks>
///     The context applies persistence mappings and dispatches domain events after changes have been saved.
///     Recorded events are cleared after the save cycle so aggregate instances do not retain stale facts
///     across units of work.
///     This is a simple in-process domain-event mechanism. It is suitable for local side effects that belong
///     to the same application. Use an outbox when event delivery must be durable or retried reliably.
/// </remarks>
public sealed partial class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
        where TAggregate : AggregateRoot
        where TKey : struct, IEntityId<TKey>
    {
        if (this is IRepository<TAggregate, TKey> repository)
        {
            return repository;
        }

        throw new InvalidOperationException(
            $"No repository is configured for aggregate '{typeof(TAggregate).Name}' with key '{typeof(TKey).Name}'.");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        IDomainEvent[] domainEvents =
        [
            .. ChangeTracker
              .Entries<AggregateRoot>()
              .SelectMany(entry => entry.Entity.DomainEvents)
        ];

        int result = await base.SaveChangesAsync(cancellationToken);

        try
        {
            if (domainEvents.Length > 0)
            {
                await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            }

            return result;
        }
        finally
        {
            ClearDomainEvents();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private void ClearDomainEvents()
    {
        foreach (EntityEntry<AggregateRoot> entry in ChangeTracker.Entries<AggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }
}
