# Company.Template

Production-oriented Clean Architecture Web API template.

This template is intentionally small enough to understand, but complete enough to show how a real API can be structured.

It combines:

- Clean Architecture boundaries
- Ports and Adapters style dependencies
- Minimal API endpoints
- explicit `Result<T>` based failure handling
- EF Core persistence
- provider-specific database materialization
- optional Keycloak JWT bearer authentication
- .NET Aspire local orchestration
- Testcontainers-based integration tests
- OpenTelemetry-ready service defaults

## Getting started

Create a new project:

```bash
dotnet new cleanapi -n MyCompany.Products --db PostgreSql
```

Run it through Aspire:

```bash
dotnet run --project src/aspire/MyCompany.Products.AppHost
```

Run tests:

```bash
dotnet test
```

## Project layout

```text
src/
  Company.Template.Api
  Company.Template.Application
  Company.Template.Domain
  Company.Template.Infrastructure
  Company.Template.CompositionRoot

  composition/
    Company.Template.Composition.Abstractions
    Company.Template.Composition.AspNetCore

  aspire/
    Company.Template.AppHost
    Company.Template.MigrationService
    Company.Template.ServiceDefaults

tests/
  Company.Template.Domain.Tests
  Company.Template.Application.Tests
  Company.Template.Infrastructure.Tests
  Company.Template.Api.Tests
  Company.Template.TestSupport
```

The root `src` projects show the application architecture. The `src/composition` projects contain the reusable composition mechanism shown as source, and `src/aspire` contains local-development orchestration support.

## Documentation map

| File                                   | Topic                                                                   |
|----------------------------------------|-------------------------------------------------------------------------|
| [ARCHITECTURE.md](ARCHITECTURE.md)     | Layering rules, dependency direction, project responsibilities          |
| [APPLICATION.md](APPLICATION.md)       | Use cases, decorators, application behavior                             |
| [API.md](API.md)                       | Minimal APIs, endpoint modules, request validation, HTTP result mapping |
| [RESULTS.md](RESULTS.md)               | `Result<T>`, `Error`, `ValidationResult<T>`, `Option<T>`                |
| [PERSISTENCE.md](PERSISTENCE.md)       | Persistence ports, EF Core adapters, command/query boundaries           |
| [OBSERVABILITY.md](OBSERVABILITY.md)   | OpenTelemetry, logs, traces, metrics                                    |
| [DATABASE.md](DATABASE.md)             | Provider selection, generated provider configuration                    |
| [ASPIRE.md](ASPIRE.md)                 | Local orchestration and startup order                                   |
| [AUTHENTICATION.md](AUTHENTICATION.md) | Optional Keycloak authentication and local realm setup                  |
| [OPENAPI.md](OPENAPI.md)               | OpenAPI document, response metadata, OAuth metadata                     |
| [MIGRATIONS.md](MIGRATIONS.md)         | EF Core migrations and production migration bundles                     |
| [TESTING.md](TESTING.md)               | Unit, integration, Testcontainers, API problem tests, smoke tests       |
| [FEATURES.md](FEATURES.md)             | How to add a new feature to the template                                |
| [FEATURE_COMPOSITION.md](docs/FEATURE_COMPOSITION.md) | Feature composition, markers, decorators, and web pipeline composition |

## Core design principles

- Keep endpoint handlers thin.
- Follow Clean Architecture with Ports-and-Adapters style boundaries.
- Put business behavior in the domain model or application use cases.
- Represent expected application failures with `Result` / `Result<T>`.
- Use the lightweight validation builder for API request validation that must collect all field errors.
- Use exceptions for unexpected failures or violated programming contracts.
- Keep the Domain layer persistence-free and infrastructure-free.

For the design rationale, start with [ARCHITECTURE.md](ARCHITECTURE.md).
