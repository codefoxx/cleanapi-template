# API

> HTTP boundary built with ASP.NET Core Minimal APIs.

## API style

Endpoint handlers should stay thin.

They translate HTTP request data into application commands or queries, execute a use case, and map the application result back to HTTP.

Do not put business logic into endpoint handlers. Business rules belong in the domain model or application use cases.

The API layer is allowed to own HTTP/request-specific validation and mapping code. This keeps transport concerns out of Application while still avoiding large endpoint handlers.

## Feature web app modules

HTTP adapter behavior is activated through feature web app modules.

A feature web app module implements `IFeatureWebAppModule<TFeature>` and extends the ASP.NET Core `WebApplication` pipeline for one explicitly selected feature:

```csharp
public sealed class ProductsWebAppModule : IFeatureWebAppModule<ProductsFeature>
{
    public void Use(FeatureWebAppContext context)
    {
        ProductEndpoints endpoints = new();

        endpoints.MapEndpoints(context.App);
    }
}
```

The composition entry point chooses which feature HTTP adapters are active:

```csharp
app
   .UseFeaturesFromAssemblies(typeof(ApiAssemblyMarker).Assembly)
   .Use<ProductsFeature>();
```

Feature web app modules are intentionally broader than endpoint modules. They can map endpoints, register middleware, or apply other HTTP adapter pipeline configuration for a selected feature.

The concrete endpoint classes can stay focused on route mapping:

```csharp
internal sealed class ProductEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("", GetProductsAsync);
    }
}
```

Handler methods can still use normal Minimal API parameter injection for:

- services
- route values
- query values
- request bodies
- cancellation tokens

Use descriptive route parameter names such as `productId` when that makes the generated OpenAPI document clearer.

## Request validation and command/query mapping

HTTP request contracts are transport input. They may contain missing, null, malformed, or otherwise invalid values.

Application commands and queries should represent validated application input. They should not reference API request types.

The API layer maps request contracts to commands and queries through small extension methods, for example:

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

This keeps the dependency direction clean:

```text
API request
  -> API mapping/validation extension
  -> Application command/query
  -> Use case
  -> Domain
```

The template uses a small dependency-free validation builder instead of FluentValidation by default. The builder executes all configured rules and collects all field-level validation errors before a command or query is created.

Use `RuleFor(...)` for independent property rules and `Rule(...)` for cross-field request rules.

## HTTP result mapping

Use cases return `Result` / `Result<T>`. The API layer maps these results to HTTP responses.

| HTTP status | Meaning |
| --- | --- |
| `400 Bad Request` | The request could not be parsed or bound by ASP.NET Core. |
| `422 Unprocessable Entity` | The request was syntactically valid, but rejected by request/application validation. |
| `404 Not Found` | The requested resource does not exist. |
| `409 Conflict` | The request conflicts with the current domain state. |
| `500 Internal Server Error` | An unexpected exception escaped application result handling. |

Expected application failures should be returned as `Result` values.

Unexpected failures should throw and are handled by the global exception handler.

## Validation problem response shape

Request validation failures use `ValidationProblemDetails`.

The top-level problem code identifies the response category:

```json
{
    "title": "Validation failed.",
    "status": 422,
    "detail": "One or more validation errors occurred.",
    "code": "validation_error",
    "errors": {
        "name": ["Product name is required."],
        "price": ["Price cannot be negative."]
    }
}
```

Field-specific errors are exposed through the `errors` object. The field name comes from the `RuleFor(...)` selector.

Domain validation that happens later in a use case may still produce a validation problem under the generic `request` target when the error does not belong to a single HTTP field.

## Mapping rule

HTTP status mapping should use `ErrorType`, not string error codes.

`Error.Code` exposes an application-owned `ErrorCode` value. It is a stable machine-readable identifier for clients, tests, logs, and diagnostics, but it should not decide transport status by itself.

`DomainErrorCode` belongs to the Domain layer. If a domain failure reaches the Application layer, the domain code value may be preserved through the `DomainError` to `Error` mapping, but the API still exposes it as an application `ErrorCode` value.

For validation responses, the top-level code is usually `validation_error`. More specific domain/application codes may still exist inside the collected validation details or in non-aggregated validation failures.

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
