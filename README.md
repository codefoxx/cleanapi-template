# cleanapi-template

> Production-oriented .NET Web API templates packaged for `dotnet new`.

This repository currently contains the Clean Architecture template in `content/`.
The generated project README and architecture documentation are stored there because they are copied into new projects.

The current template focuses on a production-oriented Clean Architecture Web API with pragmatic Ports-and-Adapters
boundaries, DDD-style domain modeling, explicit result handling, a thin EF Core-backed persistence boundary, and a
Minimal API HTTP adapter.

## Quick start

Create a project from the installed template:

```bash
dotnet new cleanapi -n Company.Template --db PostgreSql
```

Restore packages:

```bash
dotnet restore
```

Create the initial migration:

```bash
dotnet ef migrations add InitialCreate \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Api \
  --context ApplicationDbContext \
  --output-dir Persistence/Migrations
```

Run the local Aspire AppHost:

```bash
dotnet run --project src/Company.Template.AppHost
```

Run tests:

```bash
dotnet test
```

## Generated project structure

```text
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
```

## Generated documentation

| File | Topic |
| --- | --- |
| [content/README.md](content/README.md) | Generated project entry point |
| [content/ARCHITECTURE.md](content/ARCHITECTURE.md) | Layering rules, dependency direction, project responsibilities |
| [content/APPLICATION.md](content/APPLICATION.md) | Use cases, decorators, application behavior |
| [content/API.md](content/API.md) | Minimal APIs, endpoint modules, request validation, HTTP result mapping |
| [content/RESULTS.md](content/RESULTS.md) | `Result<T>`, `Error`, `ValidationResult<T>`, `Option<T>` |
| [content/PERSISTENCE.md](content/PERSISTENCE.md) | Persistence ports, EF Core adapters, command/query boundaries |
| [content/OBSERVABILITY.md](content/OBSERVABILITY.md) | OpenTelemetry, logs, traces, metrics |
| [content/DATABASE.md](content/DATABASE.md) | Provider selection, generated provider configuration |
| [content/ASPIRE.md](content/ASPIRE.md) | Local orchestration, startup order, pgAdmin |
| [content/AUTHENTICATION.md](content/AUTHENTICATION.md) | Optional Keycloak authentication and local realm setup |
| [content/OPENAPI.md](content/OPENAPI.md) | OpenAPI document, response metadata, OAuth metadata |
| [content/MIGRATIONS.md](content/MIGRATIONS.md) | EF Core migrations and production migration bundles |
| [content/TESTING.md](content/TESTING.md) | Unit, integration, Testcontainers, API problem tests, smoke tests |
| [content/FEATURES.md](content/FEATURES.md) | How to add a new feature to the template |

## Core design principles

- Keep endpoint handlers thin.
- Follow Clean Architecture with pragmatic Ports-and-Adapters style boundaries.
- Put business behavior in the domain model or application use cases.
- Represent expected application failures with `Result` / `Result<T>`.
- Use the lightweight validation builder for API request validation that must collect all field errors.
- Use exceptions for unexpected failures or violated programming contracts.
- Keep the Domain layer persistence-free and infrastructure-free.
- Use thin EF Core-friendly persistence ports for commands and named query ports for reads.
- Use relational integration tests instead of EF Core InMemory.
- Treat observability as a production concern, not an afterthought.

## Roadmap

The package is intended to contain more than one template over time.

The Clean Architecture template is finished first. A second Vertical Slice Architecture template can then be added with
the same production-oriented baseline but a different application structure.

## Central package management

Package versions are centralized in:

```text
Directory.Packages.props
```

Project files reference packages without versions.
