# Company.Template

> Production-oriented Clean Architecture Web API generated from the `cleanapi` template.

This README is the entry point for the generated project. The detailed documentation is split into focused Markdown
files so the project stays easy to read and maintain.

## Quick start

Create a project:

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

## Project structure

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
| [ASPIRE.md](ASPIRE.md)                 | Local orchestration, startup order, pgAdmin                             |
| [AUTHENTICATION.md](AUTHENTICATION.md) | Optional Keycloak authentication and local realm setup                  |
| [OPENAPI.md](OPENAPI.md)               | OpenAPI document, response metadata, OAuth metadata                     |
| [MIGRATIONS.md](MIGRATIONS.md)         | EF Core migrations and production migration bundles                     |
| [TESTING.md](TESTING.md)               | Unit, integration, Testcontainers, API problem tests, smoke tests       |
| [FEATURES.md](FEATURES.md)             | How to add a new feature to the template                                |

## Core design principles

- Keep endpoint handlers thin.
- Follow Clean Architecture with Ports-and-Adapters style boundaries.
- Put business behavior in the domain model or application use cases.
- Represent expected application failures with `Result` / `Result<T>`.
- Use the lightweight validation builder for API request validation that must collect all field errors.
- Use exceptions for unexpected failures or violated programming contracts.
- Keep the Domain layer persistence-free and infrastructure-free.
- Use thin EF Core-friendly persistence ports for commands and named query ports for reads.
- Use relational integration tests instead of EF Core InMemory.
- Treat observability as a production concern, not an afterthought.

## Central package management

Package versions are centralized in:

```text
Directory.Packages.props
```

Project files reference packages without versions.
