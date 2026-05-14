using Company.Template.Domain.Common;

namespace Company.Template.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
        where TAggregate : AggregateRoot
        where TKey : struct, IEntityId<TKey>;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
