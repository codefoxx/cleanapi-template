# RESULTS

> Explicit modeling of expected outcomes and optional values.

## Expected vs unexpected failures

Expected application outcomes are represented with `Result` / `Result<T>`.

Use this for:

- validation failures
- not found
- conflicts
- successful command/query results

Unexpected failures should still throw and be handled by the API boundary.

## Result policy

A successful result has:

```text
value
Error.None
```

A failed result has:

```text
no value
real Error
```

`Error.None` is a Null Object for successful results. It is not a valid failure error.

## Domain failure policy

Domain creation APIs follow this convention:

| API | Purpose |
| --- | --- |
| `Create(...)` / `From(...)` | Strict API. Expects valid input and throws on contract violation. |
| `TryCreate(...)` / `TryFrom(...)` | Safe API for raw input. Returns `false` and `DomainError` for expected failures. |

The Domain layer exposes `DomainError`. It does not reference application `Result<T>` or application `Error`.

Application code translates domain failures:

```text
DomainError
  -> Application Error
  -> Result<T>.Failure(...)
  -> HTTP boundary mapping
```

## Optional values

Optional values are represented with `Option<T>`.

Use `Option<T>` when absence is an expected state and should be made explicit.

Good examples:

- loading an entity that may not exist
- resolving an optional value from configuration or context
- representing an optional application-level filter after validation and normalization

Examples:

```csharp
Option<Product> maybeProduct =
    await products.FindAsync(productId, cancellationToken);
```

```csharp
Option<ProductDto> maybeProduct =
    await _productQueries.GetByIdAsync(productId, cancellationToken);
```

Use `Map` or `Bind` while staying in the option world.

Use `Match`, `TryGetValue`, or `OrElse` when leaving the option world.

Examples:

```csharp
Option<ProductDto> maybeDto = maybeProduct.Map(ProductMapper.ToDto);
```

```csharp
return maybeProduct.Match(
    some: Result<ProductDto>.Success,
    none: () => Result<ProductDto>.Failure(Error.NotFound("Product was not found.")));
```

## Option and EF Core queries

EF Core query composition belongs in Infrastructure query implementations.

When composing EF Core `IQueryable<T>` queries, unwrap `Option<T>` before building the expression.

Do not put `Option<T>.Match`, `Map`, or `Bind` inside an EF query predicate because EF Core cannot translate custom option methods to SQL.

Good:

```csharp
if (filter.Status.TryGetValue(out ProductStatus status))
{
    query = query.Where(product => product.Status == status);
}
```

Avoid:

```csharp
query = query.Where(product =>
    filter.Status.Match(
        some: status => product.Status == status,
        none: () => true));
```

## Strongly typed IDs

Domain IDs use strongly typed ID structs.

Example:

```csharp
public readonly record struct ProductId : IEntityId<ProductId>
{
    private ProductId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static ProductId New()
    {
        return new ProductId(EntityId.New());
    }

    public static ProductId From(Guid value)
    {
        return EntityId.From(
            value,
            static id => new ProductId(id),
            DomainErrorCodes.ProductIdRequired,
            "Product id is required.",
            nameof(value));
    }

    public static bool TryFrom(
        Guid value,
        out ProductId productId,
        out DomainError? error)
    {
        return EntityId.TryFrom(
            value,
            static id => new ProductId(id),
            DomainErrorCodes.ProductIdRequired,
            "Product id is required.",
            out productId,
            out error);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
```

The shared `EntityId` helper centralizes ID creation and validation.

IDs use UUID v7 for new values.

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
