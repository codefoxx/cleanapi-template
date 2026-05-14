using Company.Template.Domain.Common;

namespace Company.Template.Application.Abstractions.Persistence;

public interface IRepository<TAggregate, in TKey>
    where TAggregate : AggregateRoot
    where TKey : struct, IEntityId<TKey>
{
    Task<Option<TAggregate>> TryFindAsync(
        TKey key,
        CancellationToken cancellationToken);

    void Add(TAggregate aggregate);

    void Delete(TAggregate aggregate);
}
