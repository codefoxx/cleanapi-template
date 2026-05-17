# FEATURES

> How to add a new feature to the generated solution.

## Sample domain

The template includes a small Catalog/Product domain:

- `Product` aggregate root
- `ProductId` strongly typed ID
- `ProductName` value object
- `Money` value object
- `Currency` value object
- `ProductStatus`
- domain events:
  - `ProductCreatedDomainEvent`
  - `ProductPriceChangedDomainEvent`
  - `ProductDiscontinuedDomainEvent`

Business rules live in the domain model:

- product name must not be empty
- price must not be negative
- currency must be supported by the application
- discontinued products cannot be renamed
- discontinued products cannot have their price changed
- changing price to the same value does nothing
- discontinuing an already discontinued product is idempotent
- domain events are raised only when state changes

## Adding a new feature

1. Put business invariants and behavior in `Domain`.
2. Add commands, queries, and use cases in `Application`.
3. Implement `IUseCase<TRequest, TResult>` or `IUseCase<TRequest>`.
4. For command use cases, expose aggregate access through `IRepository<TAggregate, TKey>` via `IUnitOfWork`.
5. For query use cases, define named query ports in `Application`, for example `IProductQueries`.
6. Implement persistence adapters in `Infrastructure`.
7. Add EF Core mapping in `Infrastructure`.
8. Add API request/response DTOs and endpoint modules under `Api/Endpoints/{Feature}`.
9. Add API request-to-command/query extension methods when request validation or mapping is needed.
10. Add tests at the appropriate layer.
11. Add feature-specific logs only for meaningful business decisions.

## Feature rules

Keep endpoint handlers thin.

Do not expose domain entities or EF entities directly from the API.

Application commands and queries should not depend on API request contracts.

Expected request-validation failures should flow through:

```text
HTTP request DTO
  -> API request validation extension
  -> ValidationResult<T>
  -> Result<T>.Failure(validation_error with details)
  -> HTTP boundary translation
```

Expected user-input failures that are domain-specific and not caught by request validation should flow through:

```text
raw command/query value
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

## API validation extension example

```csharp
internal static class CreateProductRequestExtensions
{
    public static Result<CreateProductCommand> ToCommand(this CreateProductRequest request)
    {
        return Validation
            .For(request)
            .RuleFor(x => x.Name, ValidateName)
            .RuleFor(x => x.Price, ValidatePrice)
            .RuleFor(x => x.Currency, ValidateCurrency)
            .Map(CreateCommand)
            .ToResult();
    }
}
```

Use `RuleFor(...)` for field-level validation and `Rule(...)` for request-level or cross-field validation.

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
