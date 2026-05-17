# RESULTS

> Explicit modeling of expected outcomes, validation failures, and optional values.

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

`Result<T>` should protect its own invariants:

- `Success(...)` requires a non-null value
- `Failure(...)` requires a real error
- accessing `Value` on a failure is invalid
- accessing failure information on a success should return `Error.None`

## Error model

`Error` represents an expected application failure.

It contains:

```text
ErrorType
DomainErrorCode
Message
Target?
Details?
```

`ErrorType` decides the HTTP status at the API boundary.

`DomainErrorCode` is a stable machine-readable code. It is useful for tests, clients, logs, and diagnostics, but it should not decide the HTTP status by itself.

`Target` identifies the request field or logical input that caused a validation failure.

`Details` is used for aggregated validation failures. The top-level error usually has code `validation_error`; the details contain the individual field errors.

## Functional composition

`Result<T>` supports functional composition for expected application flows:

```csharp
Result<ProductDto> result = await request
    .ToCommand()
    .BindAsync(command => useCase.ExecuteAsync(command, cancellationToken));
```

Use:

| Method | Purpose |
| --- | --- |
| `Map` | Transform a successful value while keeping failures unchanged. |
| `Bind` | Continue with another result-producing operation. |
| `BindAsync` | Continue with an asynchronous result-producing operation. |
| `Match` | Leave the result world and handle both success and failure explicitly. |

`Bind` and `BindAsync` are fail-fast. They do not collect multiple validation errors.

## Request validation results

Web/API request validation often needs to report all invalid fields at once.

For that, the template uses a small dependency-free validation builder:

```csharp
return Validation
    .For(request)
    .RuleFor(x => x.Name, ValidateName)
    .RuleFor(x => x.Price, ValidatePrice)
    .RuleFor(x => x.Currency, ValidateCurrency)
    .Map(CreateCommand)
    .ToResult();
```

This produces a `ValidationResult<T>` internally.

`ValidationResult<T>` is not a general application result. It is a short-lived helper for request validation:

```text
ValidationResult<T>
  -> collects all field errors
  -> maps only when all rules passed
  -> converts to Result<T>
```

The converted `Result<T>` contains one top-level validation error with `Details` for each field error.

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

Domain operation failures use `DomainResult` / `DomainResult<T>` when an aggregate or value object needs to report an expected business-rule failure without throwing.

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

Do not use `Option<T>` for validation failures. Validation failures need error codes, messages, targets, and sometimes details. Use `Result<T>` or `ValidationResult<T>` instead.

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
