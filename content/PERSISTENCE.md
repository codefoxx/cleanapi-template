# PERSISTENCE

> EF Core is used directly and intentionally at the application boundary.

## Persistence style

The template avoids custom repositories by design.

Instead:

- `DbContext` is the unit of work.
- `DbSet<TEntity>` is the repository.
- Read queries use `IQueryable<T>` and `AsNoTracking()`.
- Feature-specific DbContext interfaces expose only the DbSets and query roots needed by that feature.
- Query extension methods provide named, composable queries.
- Query result helpers can return `Option<T>` when absence is expected.

## Read example

```csharp
Option<Product> product = await _dbContext.ProductsForRead
    .WithId(productId)
    .SingleOrNoneAsync(cancellationToken);
```

## Write example

```csharp
_dbContext.Products.Add(product);

await _dbContext.SaveChangesAsync(cancellationToken);
```

## Design trade-off

This keeps EF Core visible where it is useful, while keeping the Domain layer persistence-free.

The Application layer may compose queries and expose feature-specific persistence needs through small interfaces.

The Domain layer must not depend on EF Core.

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
