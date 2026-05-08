using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Company.Template.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDomainEventDispatcher? domainEventDispatcher = null)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public DbSet<Product> Products => Set<Product>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_domainEventDispatcher is not null && domainEvents.Length > 0)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
            {
                entry.Entity.ClearDomainEvents();
            }
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
