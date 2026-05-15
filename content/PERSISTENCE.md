# PERSISTENCE

> Thin EF Core-friendly persistence boundaries for commands and queries.

## Persistence style

The template uses EF Core as the concrete persistence technology.

It does not build a large repository, unit-of-work, or specification framework around EF Core.

Instead, it separates command and query persistence needs:

| Use case type | Application port | Infrastructure adapter |
| --- | --- | --- |
| Commands | `IUnitOfWork`, `IRepository<TAggregate, TKey>` | `ApplicationDbContext` with tracked EF Core aggregates |
| Queries | named query interfaces such as `IProductQueries` | EF Core LINQ queries, projections, `AsNoTracking()` |

## Command persistence

Command use cases load aggregates, call domain behavior, and commit changes.

They use:

```text
IUnitOfWork
IRepository<TAggregate, TKey>
```

Example:

```csharp
IRepository<Product, ProductId> products =
    _unitOfWork.GetRepository<Product, ProductId>();

Option<Product> maybeProduct =
    await products.FindAsync(productId, cancellationToken);

if (!maybeProduct.TryGetValue(out Product? product))
{
    return Result<ProductDto>.Failure(Error.NotFound("Product was not found."));
}

DomainResult changePriceResult = product.ChangePrice(money, _clock.UtcNow);

if (changePriceResult.IsFailure)
{
    return Result<ProductDto>.Failure(changePriceResult.Error.ToApplicationError());
}

await _unitOfWork.SaveChangesAsync(cancellationToken);
```

The repository abstraction is intentionally small:

```text
FindAsync
Add
Delete
```

There is no `Update` method because EF Core tracks loaded aggregates.

Command repositories load tracked aggregates because the use case intends to modify them.

## Query persistence

Query use cases should not load aggregates through repositories.

Queries often need:

- filtering
- sorting
- paging
- projections
- joins
- read-model shaping
- aggregate-boundary crossing

For that reason, query use cases depend on named query ports:

```csharp
public interface IProductQueries : IQuery
{
    Task<Option<ProductDto>> GetByIdAsync(
        ProductId productId,
        CancellationToken cancellationToken);

    Task<PagedResult<ProductDto>> GetProductsAsync(
        ProductFilter filter,
        ProductSort sort,
        PageRequest page,
        CancellationToken cancellationToken);
}
```

Infrastructure implements these query ports with EF Core.

Read-only queries should use `AsNoTracking()`.

## Registration

Application query interfaces implement the `IQuery` marker interface.

Infrastructure registers query implementations by scanning for classes that implement those query ports.

This keeps the composition root small while still making query dependencies explicit in the Application layer.

## Design trade-off

This is a Ports-and-Adapters style boundary, but it is intentionally not technology-neutral fantasy.

EF Core remains the concrete persistence technology.

The template abstracts use-case dependencies, not every EF Core concept.

The intended trade-off is:

- use cases do not depend directly on `DbContext`, `DbSet<T>`, or `IQueryable<T>`
- Infrastructure can still use EF Core, LINQ, tracking, projections, and migrations effectively
- Domain remains persistence-free
- no generic specification framework is introduced

---

## Related documents

- [README](README.md)
- [ARCHITECTURE](ARCHITECTURE.md)
- [APPLICATION](APPLICATION.md)
- [API](API.md)
- [RESULTS](RESULTS.md)
- [PERSISTENCE](PERSISTENCE.md)
- [OBSERVABILITY](OBSERVABILITY.md)
- [DATABASE](DATABASE.md)
- [ASPIRE](ASPIRE.md)
- [AUTHENTICATION](AUTHENTICATION.md)
- [OPENAPI](OPENAPI.md)
- [MIGRATIONS](MIGRATIONS.md)
- [TESTING](TESTING.md)
- [FEATURES](FEATURES.md)
