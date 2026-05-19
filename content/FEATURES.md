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
2. Add an explicit feature marker implementing `IFeature`, for example `ProductsFeature`.
3. Add commands, queries, and use cases in `Application`.
4. Implement `IUseCase<TRequest, TResult>` or `IUseCase<TRequest>`.
5. Register application use cases in an `IFeatureServiceModule<TFeature>` implementation.
6. For command use cases, expose aggregate access through `IRepository<TAggregate, TKey>` via `IUnitOfWork`.
7. For query use cases, define named query ports in `Application`, for example `IProductQueries`.
8. Implement persistence adapters in `Infrastructure`.
9. Register infrastructure adapters in an `IFeatureServiceModule<TFeature>` implementation.
10. Add EF Core mapping in `Infrastructure`.
11. Add API request/response DTOs and endpoint classes under `Api/Endpoints/{Feature}`.
12. Add an `IFeatureWebAppModule<TFeature>` implementation that activates the feature's HTTP adapter pipeline.
13. Add API request-to-command/query extension methods when request validation or mapping is needed.
14. Activate the feature in the composition entry point through `.ComposeFeatures(...)` and `.Use<TFeature>()`.
15. Add tests at the appropriate layer.
16. Add feature-specific logs only for meaningful business decisions.

## Feature composition

Features are activated explicitly by the composition entry point.

Service-side modules register application and infrastructure services:

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

WebApplication-side modules activate HTTP adapter pipeline changes:

```csharp
app
   .UseFeaturesFromAssemblies(typeof(ApiAssemblyMarker).Assembly)
   .Use<ProductsFeature>();
```

A feature can have modules in multiple assemblies. The feature marker links them together without relying on naming conventions.

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
- [FEATURE_COMPOSITION](docs/FEATURE_COMPOSITION.md)
