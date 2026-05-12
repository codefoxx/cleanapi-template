using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;


namespace Company.Template.Infrastructure.Persistence;

/// <summary>
/// EF Core unit-of-work implementation for the template application.
/// </summary>
/// <remarks>
/// The context applies persistence mappings and dispatches domain events only after changes have been saved. Recorded
/// events are cleared after the save cycle so aggregate instances do not retain stale facts across units of work.
/// </remarks>
public sealed partial class ApplicationDbContext : DbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<IDomainEvent> domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToArray();

        int result = await base.SaveChangesAsync(cancellationToken);

        if (domainEvents.Count > 0)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        foreach (EntityEntry<AggregateRoot> entry in ChangeTracker.Entries<AggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }
}
