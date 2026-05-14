# API

> HTTP boundary built with ASP.NET Core Minimal APIs.

## API style

Endpoint handlers should stay thin.

They translate HTTP request data into application commands or queries, execute a use case, and map the application result back to HTTP.

Do not put business logic into endpoint handlers. Business rules belong in the domain model or application use cases.

## Endpoint modules

Endpoints are grouped into endpoint modules.

Each module implements `IEndpointModule` and registers its own routes:

```csharp
internal sealed class ProductEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", GetProductsAsync);
    }
}
```

`Program.cs` scans endpoint modules from the API assembly:

```csharp
app.MapEndpointModulesFromAssembly<ApiAssemblyMarker>();
```

This keeps `Program.cs` small while still using standard ASP.NET Core Minimal APIs.

If endpoints are split across multiple assemblies later, map each assembly explicitly:

```csharp
app.MapEndpointModulesFromAssembly<ApiAssemblyMarker>()
   .MapEndpointModulesFromAssembly<AdminApiAssemblyMarker>();
```

Endpoint modules are created by reflection and should have a parameterless constructor.

Handler methods can still use normal Minimal API parameter injection for:

- services
- commands
- route values
- query values
- cancellation tokens

## HTTP result mapping

Use cases return `Result` / `Result<T>`. The API layer maps these results to HTTP responses.

| HTTP status | Meaning |
| --- | --- |
| `400 Bad Request` | The request could not be parsed or bound by ASP.NET Core. |
| `422 Unprocessable Entity` | The request was syntactically valid, but rejected by application validation. |
| `404 Not Found` | The requested resource does not exist. |
| `409 Conflict` | The request conflicts with the current domain state. |
| `500 Internal Server Error` | An unexpected exception escaped application result handling. |

Expected application failures should be returned as `Result` values.

Unexpected failures should throw and are handled by the global exception handler.

## Mapping rule

HTTP status mapping should use `ErrorType`, not string error codes.

`Error.Code` is a stable machine-readable identifier. It should not decide transport status by itself.

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
