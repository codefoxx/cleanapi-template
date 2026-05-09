# Company.Template

Production-oriented Clean Architecture Web API generated from the `cleanapi` template.

## Project structure

~~~text
src/
  Company.Template.Api
  Company.Template.Application
  Company.Template.Domain
  Company.Template.Infrastructure
  Company.Template.ServiceDefaults
  Company.Template.AppHost

tests/
  Company.Template.Domain.Tests
  Company.Template.Application.Tests
  Company.Template.Infrastructure.Tests
  Company.Template.Api.Tests
~~~

## Architecture rules

- `Domain` references no other project.
- `Application` references `Domain` and EF Core abstractions.
- `Infrastructure` references `Application` and `Domain`.
- `Api` references `Application`, `Infrastructure`, and `ServiceDefaults`.
- `AppHost` is used for local orchestration with .NET Aspire.
- Tests reference only the projects they need.

The Domain layer must not reference EF Core, ASP.NET Core, Keycloak, Aspire, or any other infrastructure concern.

The Application layer intentionally uses EF Core query abstractions. This template treats `DbSet<TEntity>` as the repository and `DbContext` as the unit of work. It does not add repository or unit-of-work wrappers around EF Core.

## Persistence style

The template avoids custom repositories by design.

Instead:

- `DbContext` is the unit of work.
- `DbSet<TEntity>` is the repository.
- Read queries use `IQueryable<T>` and `AsNoTracking()`.
- Feature-specific DbContext interfaces expose only the DbSets and query roots needed by that feature.
- Query extension methods provide named, composable queries.

Example:

~~~csharp
Product? product = await _dbContext.ProductsForRead
    .WithId(productId)
    .SingleOrDefaultAsync(cancellationToken);
~~~

For writes, use the tracked DbSet and commit through the DbContext:

~~~csharp
_dbContext.Products.Add(product);

await _dbContext.SaveChangesAsync(cancellationToken);
~~~

This keeps EF Core visible where it is useful, while keeping the Domain layer persistence-free.

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
- changing price to the same value does nothing
- domain events are raised only when state changes

## Database provider selection

The provider is selected when the project is generated:

~~~bash
dotnet new cleanapi -n Company.Template --db PostgreSql
~~~

Valid provider values:

- `PostgreSql`
- `SqlServer`
- `MySql`

The selected provider is written to configuration:

~~~json
{
  "Database": {
    "Provider": "__DB_PROVIDER__",
    "ConnectionStringName": "DefaultConnection"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Set by Aspire AppHost. Replace this when running the API directly."
  }
}
~~~

Provider-specific EF Core configuration is isolated in:

~~~text
src/Company.Template.Infrastructure/Persistence/Providers/
~~~

Only the selected provider is compiled into the generated project.

## Running with Aspire

Run:

~~~bash
dotnet run --project src/Company.Template.AppHost
~~~

Aspire starts the selected database container and wires the connection string to the API.

Local development tools can be enabled in:

~~~text
src/Company.Template.AppHost/appsettings.json
~~~

Example:

~~~json
{
  "AppHost": {
    "StartPgAdmin": false,
    "StartKeycloak": false
  }
}
~~~

## Optional pgAdmin

pgAdmin is disabled by default.

Enable it in the AppHost configuration:

~~~json
{
  "AppHost": {
    "StartPgAdmin": true
  }
}
~~~

pgAdmin is intended as a local development tool only. It is not part of the application architecture.

## Optional Keycloak authentication

Authentication is disabled by default:

~~~json
{
  "Authentication": {
    "Enabled": false
  }
}
~~~

Enable local Keycloak orchestration in the AppHost:

~~~json
{
  "AppHost": {
    "StartKeycloak": true
  }
}
~~~

When Keycloak is started by Aspire, the AppHost wires the required authentication settings into the API.

The API validates bearer tokens. It does not perform browser login and does not use cookie authentication.

Example authorization policies:

- `products.read`
- `products.write`

## OpenAPI

In development:

~~~text
/openapi/v1.json
~~~

The OpenAPI document includes bearer-token metadata for secured endpoint testing.

## Migrations

Add migrations from the Infrastructure project with the API as startup project:

~~~bash
dotnet ef migrations add InitialCreate --project src/Company.Template.Infrastructure --startup-project src/Company.Template.Api --output-dir Persistence/Migrations
~~~

Apply migrations:

~~~bash
dotnet ef database update --project src/Company.Template.Infrastructure --startup-project src/Company.Template.Api
~~~

For local Aspire runs, keep migration execution explicit unless your team intentionally adds development-only automatic migration execution.

## Tests

Run:

~~~bash
dotnet test
~~~

Integration tests use Testcontainers and the selected relational database provider.

Do not use EF Core InMemory as a substitute for relational integration tests.

Tests follow an Arrange / Act / Assert structure and use descriptive names such as:

~~~text
CreateProduct_WhenRequestIsValid_ShouldCreateProduct
GetProductById_WhenProductExists_ShouldReturnProduct
SaveChanges_WhenProductIsAdded_ShouldPersistAndReloadProduct
~~~

The goal is that tests act as executable documentation.

## Central package management

Package versions are centralized in:

~~~text
Directory.Packages.props
~~~

Project files reference packages without versions.

## Adding a new feature

1. Put business invariants and behavior in `Domain`.
2. Add use cases in `Application`.
3. Add feature-specific DbContext interfaces and query extensions in `Application` when persistence access is needed.
4. Add EF Core mapping in `Infrastructure`.
5. Implement feature-specific DbContext partials in `Infrastructure`.
6. Add API request/response DTOs and endpoints under `Api/Endpoints/{Feature}`.
7. Add tests at the appropriate layer.

Keep endpoint handlers thin. Do not expose domain entities or EF entities directly from the API.
