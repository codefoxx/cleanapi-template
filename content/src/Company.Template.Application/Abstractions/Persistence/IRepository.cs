using Company.Template.Domain.Common;

namespace Company.Template.Application.Abstractions.Persistence;

/// <summary>
///     Defines the small command-side aggregate repository surface used by application use cases.
/// </summary>
/// <remarks>
///     The repository is intentionally thin. It exposes only aggregate loading and collection-style changes needed by
///     command workflows, while EF Core tracking and query composition remain Infrastructure concerns.
/// </remarks>
public interface IRepository<TAggregate, in TKey>
    where TAggregate : AggregateRoot
    where TKey : struct, IEntityId<TKey>
{
    Task<Option<TAggregate>> FindAsync(
        TKey key,
        CancellationToken cancellationToken);

    void Add(TAggregate aggregate);

    void Delete(TAggregate aggregate);
}