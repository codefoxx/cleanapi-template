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
- `Application` references `Domain` only.
- `Infrastructure` references `Application` and `Domain`.
- `Api` references `Application`, `Infrastructure`, and `ServiceDefaults`.
- `AppHost` is used for local orchestration with .NET Aspire.
- Tests reference only the projects they need.

The Domain layer must not reference EF Core, ASP.NET Core, Keycloak, Aspire, or any other infrastructure concern.

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

Configure the provider in `src/Company.Template.Api/appsettings.json`:

~~~json
{
  "Database": {
    "Provider": "__DB_PROVIDER__"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Set by Aspire AppHost. Replace this when running the API directly."
  }
}
~~~

Valid provider values:

- `PostgreSql`
- `SqlServer`
- `MySql`

Provider selection is centralized in:

~~~text
src/Company.Template.Infrastructure/Persistence/DatabaseRegistrationExtensions.cs
~~~

## Running with Aspire

Run:

~~~bash
dotnet run --project src/Company.Template.AppHost
~~~

Change the local provider in:

~~~text
src/Company.Template.AppHost/Program.cs
~~~

~~~csharp
const string databaseProvider = "__DB_PROVIDER__";
const bool enableKeycloak = false;
~~~

Aspire starts the selected database container and wires the connection string to the API.

## Optional Keycloak authentication

Authentication is disabled by default:

~~~json
{
  "Authentication": {
    "Enabled": false
  }
}
~~~

Enable it with:

~~~json
{
  "Authentication": {
    "Enabled": true,
    "Authority": "http://localhost:8080/realms/company-template",
    "Audience": "company-template-api",
    "RequireHttpsMetadata": false,
    "RoleClaimType": "roles"
  }
}
~~~

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

Integration tests use Testcontainers and PostgreSQL.

Do not use EF Core InMemory as a substitute for relational integration tests.

## Central package management

Package versions are centralized in:

~~~text
Directory.Packages.props
~~~

Project files reference packages without versions.

## Adding a new feature

1. Put business invariants and behavior in `Domain`.
2. Add use cases in `Application`.
3. Add EF Core mapping and persistence implementation in `Infrastructure` only when needed.
4. Add API request/response DTOs and endpoints under `Api/Endpoints/{Feature}`.
5. Add tests at the appropriate layer.

Keep endpoint handlers thin. Do not expose domain entities or EF entities directly from the API.
