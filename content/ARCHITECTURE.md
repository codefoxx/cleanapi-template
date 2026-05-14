# ARCHITECTURE

> Layering rules and project responsibilities for the generated solution.

## Project responsibilities

| Project | Responsibility |
| --- | --- |
| `Company.Template.Domain` | Business model, invariants, value objects, domain events |
| `Company.Template.Application` | Use cases, application contracts, result handling, query composition |
| `Company.Template.Infrastructure` | EF Core DbContext, persistence configuration, provider-specific setup |
| `Company.Template.Api` | HTTP boundary, endpoint modules, authentication, OpenAPI |
| `Company.Template.MigrationService` | One-shot EF Core migration runner |
| `Company.Template.ServiceDefaults` | Shared hosting, telemetry, and resilience defaults |
| `Company.Template.AppHost` | Local orchestration with .NET Aspire |

## Dependency rules

- `Domain` references no other project.
- `Application` references `Domain`, EF Core abstractions, and application-level infrastructure contracts.
- `Infrastructure` references `Application` and `Domain`.
- `Api` references `Application`, `Infrastructure`, and `ServiceDefaults`.
- `MigrationService` references `Infrastructure` and `ServiceDefaults`.
- `AppHost` is used for local orchestration with .NET Aspire.
- Tests reference only the projects they need.

## Domain boundary

The Domain layer must not reference:

- EF Core
- ASP.NET Core
- Keycloak
- Aspire
- OpenTelemetry
- Serilog
- any other infrastructure concern

The Domain layer may expose domain concepts such as:

- aggregates
- value objects
- strongly typed IDs
- domain events
- domain errors

## Application boundary

The Application layer intentionally uses EF Core query abstractions.

This template treats:

```text
DbSet<TEntity> as repository
DbContext       as unit of work
```

It does not add repository or unit-of-work wrappers around EF Core.

This is a deliberate trade-off. EF Core remains visible where it provides value, while the Domain layer stays persistence-free.

## Migration service

The MigrationService is an executable one-shot process.

It applies EF Core migrations and exits. It is used by Aspire so the database schema is updated before the API starts.

The local startup order is:

```text
database
  -> migration service
  -> api
```

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
