# APPLICATION

> Application behavior is implemented as explicit use cases.

## Use case interfaces

Use cases implement one of these interfaces:

```csharp
public interface IUseCase<in TRequest, TResult>
{
    Task<Result<TResult>> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IUseCase<in TRequest>
{
    Task<Result> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}
```

## Endpoint dependency style

Endpoints depend on use-case interfaces instead of concrete classes.

This allows cross-cutting decorators for:

- telemetry
- logging
- metrics
- tracing

Example:

```csharp
private static async Task<IResult> DiscontinueProductAsync(
    Guid id,
    IUseCase<DiscontinueProductCommand> useCase,
    CancellationToken cancellationToken)
{
    Result result = await useCase.ExecuteAsync(
        new DiscontinueProductCommand(id),
        cancellationToken);

    return result.ToHttpResult();
}
```

## Command and query persistence ports

Use cases depend on application ports, not on Infrastructure classes.

Command use cases use:

```text
IUnitOfWork
IRepository<TAggregate, TKey>
```

Query use cases use named query ports, for example:

```text
IProductQueries
```

This keeps use cases independent from direct EF Core access while still allowing Infrastructure to implement persistence efficiently with EF Core.

Commands and queries are intentionally treated differently:

- commands load and mutate aggregates
- queries project read models and may use optimized EF Core queries in Infrastructure

## Registration

Use cases are registered automatically via Scrutor.

The template decorates them with telemetry behavior. Generic execution telemetry belongs in:

```text
src/Company.Template.Application/Telemetry/
```

Application telemetry definitions belong in:

```text
src/Company.Template.Application/Diagnostics/
```

Feature-specific business logs should live with the feature, for example:

```text
src/Company.Template.Application/Products/
```

## Expected application outcomes

Use cases return `Result` / `Result<T>` for expected outcomes:

- validation failures
- not found
- conflicts
- successful command/query results

Unexpected failures should still throw and are handled by the API boundary.

## Use case style

Use cases should:

- validate raw request values before calling strict domain APIs
- call `TryCreate` / `TryFrom` for expected validation paths
- return `Result<T>.Failure(...)` for expected failures
- avoid catching domain exceptions as the normal validation mechanism
- delegate business behavior to the domain model
- keep command persistence commits explicit through `IUnitOfWork`
- keep query persistence behind named query ports

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
