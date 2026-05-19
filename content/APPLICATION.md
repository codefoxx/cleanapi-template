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

Commands and queries are application input models. They should not depend on API request contracts.

## Endpoint dependency style

Endpoints depend on use-case interfaces instead of concrete classes.

This allows cross-cutting decorators for:

- telemetry
- logging
- metrics
- tracing

Example:

```csharp
private static Task<IResult> CreateProductAsync(
    CreateProductRequest request,
    IUseCase<CreateProductCommand, ProductDto> useCase,
    CancellationToken cancellationToken)
{
    return request
        .ToCommand()
        .BindAsync(command => useCase.ExecuteAsync(command, cancellationToken))
        .ToHttpResultAsync(product =>
            Results.Created(
                ApiRoutes.Products.Location(product.Id),
                ProductEndpointMapper.ToResponse(product)));
}
```

The API extension method owns request-to-command mapping. The Application layer owns the command and the use case.

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

- commands load tracked aggregates and call domain behavior
- queries project read models and may use optimized EF Core queries in Infrastructure

## Registration

Feature-specific use cases are registered by application feature modules.

For example, the Products feature registers its use cases in:

```text
src/Company.Template.Application/Products/ProductsApplicationModule.cs
```

The composition entry point activates selected feature modules explicitly:

```csharp
builder.Services
       .AddFeatureServicesFromAssemblies(
            typeof(ApiAssemblyMarker).Assembly,
            typeof(ApplicationAssemblyMarker).Assembly,
            typeof(InfrastructureAssemblyMarker).Assembly)
       .WithConfiguration(builder.Configuration)
       .ComposeFeatures(features => features
           .AddTemplateDefaults()
           .AddProductCatalog()
           .DecorateUseCasesWithTelemetry());
```

The template decorates registered use cases with telemetry behavior. Generic execution telemetry belongs in:

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

Application `Result<T>` is fail-fast. It represents one application outcome. Request validation that needs to collect multiple field errors uses `ValidationResult<T>` internally and is converted to a failed `Result<T>` before leaving the API mapping step.

## Request validation vs use-case guards

The API boundary owns HTTP request validation and request-to-command/query mapping.

That means use cases should not depend on API request DTOs and should not repeat transport-specific validation just to produce field-level HTTP errors.

Use cases still remain the final application guard. Commands and queries are cleaner than raw HTTP requests, but they are not domain objects. When a use case creates strongly typed IDs or value objects from command values, it should still use safe domain APIs such as `TryCreate` or `TryFrom` and translate expected domain failures into application `Result` failures.

## Use case style

Use cases should:

- receive validated commands or queries from the API boundary where possible
- call `TryCreate` / `TryFrom` for expected domain validation paths
- return `Result<T>.Failure(...)` for expected failures
- avoid catching domain exceptions as the normal validation mechanism
- delegate business behavior to the domain model
- keep command persistence commits explicit through `IUnitOfWork`
- keep query persistence behind named query ports

Use cases should describe workflow coordination, domain delegation, and persistence decisions. They should not describe themselves as handling request-level validation when that concern belongs to the API boundary.

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
- [FEATURE_COMPOSITION](docs/FEATURE_COMPOSITION.md)
