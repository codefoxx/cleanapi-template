# Company.Template

Production-oriented Clean Architecture Web API generated from the `cleanapi` template.

## Project structure

~~~text
src/
  Company.Template.Api
  Company.Template.Application
  Company.Template.Domain
  Company.Template.Infrastructure
  Company.Template.MigrationService
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
- `MigrationService` references `Infrastructure` and `ServiceDefaults`.
- `AppHost` is used for local orchestration with .NET Aspire.
- Tests reference only the projects they need.

The Domain layer must not reference EF Core, ASP.NET Core, Keycloak, Aspire, or any other infrastructure concern.

The Application layer intentionally uses EF Core query abstractions. This template treats `DbSet<TEntity>` as the repository and `DbContext` as the unit of work. It does not add repository or unit-of-work wrappers around EF Core.

The MigrationService is an executable one-shot process. It applies EF Core migrations and exits. It is used by Aspire so the database schema is updated before the API starts.

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

## Initial setup

After creating a new project, restore packages:

~~~bash
dotnet restore
~~~

Then create the initial EF Core migration:

~~~bash
dotnet ef migrations add InitialCreate \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Api \
  --context ApplicationDbContext \
  --output-dir Persistence/Migrations
~~~

The migration files are created in:

~~~text
src/Company.Template.Infrastructure/Persistence/Migrations/
~~~

Creating migration files does not require the database container to be running. EF Core only needs to build the project, create the DbContext at design time, and compare the current model with the model snapshot.

After the initial migration exists, run the AppHost:

~~~bash
dotnet run --project src/Company.Template.AppHost
~~~

## Running with Aspire

Run:

~~~bash
dotnet run --project src/Company.Template.AppHost
~~~

Aspire starts the selected database container, runs the migration service, and then starts the API.

The local startup order is:

~~~text
database
  -> migration service
  -> api
~~~

The migration service applies pending EF Core migrations and exits. The API waits until the migration service has completed successfully.

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

The template uses EF Core migrations for relational schema management.

The `MigrationService` project is used for local Aspire development. It applies pending migrations before the API starts.

### Creating the initial migration

After generating a project, create the initial migration once:

~~~bash
dotnet ef migrations add InitialCreate \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Api \
  --context ApplicationDbContext \
  --output-dir Persistence/Migrations
~~~

Then start the AppHost:

~~~bash
dotnet run --project src/Company.Template.AppHost
~~~

The migration service will apply the migration automatically during Aspire startup.

### Adding later migrations

After changing the EF Core model, add a new migration:

~~~bash
dotnet ef migrations add DescribeYourChange \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Api \
  --context ApplicationDbContext \
  --output-dir Persistence/Migrations
~~~

Then restart the AppHost. The migration service applies the pending migration.

### Applying migrations manually

You can also apply migrations manually:

~~~bash
dotnet ef database update \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Api \
  --context ApplicationDbContext
~~~

When running with Aspire locally, manual migration execution is usually not needed because the migration service handles it.

### Migration bundles

For release pipelines, prefer EF Core migration bundles over running migrations from the API at startup.

Create a migration bundle:

~~~bash
dotnet ef migrations bundle \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Api \
  --context ApplicationDbContext \
  --output artifacts/efbundle
~~~

For a Linux self-contained bundle:

~~~bash
dotnet ef migrations bundle \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Api \
  --context ApplicationDbContext \
  --self-contained \
  --runtime linux-x64 \
  --output artifacts/efbundle
~~~

Run the bundle with a deployment connection string:

~~~bash
./artifacts/efbundle --connection "$CONNECTION_STRING"
~~~

Recommended production flow:

~~~text
build application
build migration bundle
apply migration bundle to database
deploy or start api
~~~

The migration service is mainly intended for local Aspire development. A release pipeline should apply migrations explicitly before the API is deployed or started.

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
