# FEATURES

> How to add a new feature to the generated solution.

## Sample domain

The template includes a small Catalog/Product domain:

- `Product` aggregate root
- `ProductId` strongly typed ID
- `ProductName` value object
- `Money` value object
- `ProductStatus`
- domain events:
  - `ProductCreatedDomainEvent`
  - `ProductPriceChangedDomainEvent`
  - `ProductDiscontinuedDomainEvent`

Business rules live in the domain model:

- product name must not be empty
- price must not be negative
- discontinued products cannot be renamed
- discontinued products cannot have their price changed
- changing price to the same value does nothing
- discontinuing an already discontinued product is idempotent
- domain events are raised only when state changes

## Adding a new feature

1. Put business invariants and behavior in `Domain`.
2. Add request records and use cases in `Application`.
3. Implement `IUseCase<TRequest, TResult>` or `IUseCase<TRequest>`.
4. For command use cases, expose aggregate access through `IRepository<TAggregate, TKey>` via `IUnitOfWork`.
5. For query use cases, define named query ports in `Application`, for example `IProductQueries`.
6. Implement persistence adapters in `Infrastructure`.
7. Add EF Core mapping in `Infrastructure`.
8. Add API request/response DTOs and endpoint modules under `Api/Endpoints/{Feature}`.
9. Add tests at the appropriate layer.
10. Add feature-specific logs only for meaningful business decisions.

## Feature rules

Keep endpoint handlers thin.

Do not expose domain entities or EF entities directly from the API.

Expected user-input failures should flow through:

```text
raw input
  -> TryCreate / TryFrom
  -> DomainError
  -> Application Error
  -> Result<T>.Failure
  -> HTTP boundary translation
```

Expected domain operation failures should flow through:

```text
domain operation
  -> DomainResult
  -> DomainError
  -> Application Error
  -> Result<T>.Failure
  -> HTTP boundary translation
```

Unexpected failures may still throw and are handled by the API boundary.

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
