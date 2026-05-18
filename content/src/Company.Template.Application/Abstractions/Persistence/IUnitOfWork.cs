using Company.Template.Domain.Common;

namespace Company.Template.Application.Abstractions.Persistence;

/// <summary>
///     Defines the command-side persistence boundary used to load repositories and commit aggregate changes.
/// </summary>
/// <remarks>
///     Use cases depend on this boundary so they can coordinate domain changes without depending on EF Core directly.
///     The concrete Infrastructure adapter decides how repositories and commits are implemented.
/// </remarks>
public interface IUnitOfWork
{
    IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
        where TAggregate : AggregateRoot
        where TKey : struct, IEntityId<TKey>;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}