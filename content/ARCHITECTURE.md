# ARCHITECTURE

> Layering rules, dependency direction, and Ports-and-Adapters style boundaries for the generated solution.

## Project responsibilities

| Project | Responsibility |
| --- | --- |
| `Company.Template.Domain` | Business model, invariants, value objects, domain events, domain errors |
| `Company.Template.Application` | Use cases, application ports, result handling, command/query contracts |
| `Company.Template.Infrastructure` | EF Core DbContext, persistence adapters, provider-specific setup |
| `Company.Template.Api` | HTTP adapter, endpoint modules, authentication, OpenAPI |
| `Company.Template.MigrationService` | One-shot EF Core migration runner |
| `Company.Template.ServiceDefaults` | Shared hosting, telemetry, and resilience defaults |
| `Company.Template.AppHost` | Local orchestration with .NET Aspire |

## Dependency rules

- `Domain` references no other project.
- `Application` references `Domain` and defines application-level ports.
- `Infrastructure` references `Application` and `Domain` and implements outbound ports.
- `Api` references `Application`, `Infrastructure`, and `ServiceDefaults` and acts as an HTTP adapter.
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
- domain results

## Ports and adapters style boundaries

The template follows Clean Architecture with Ports-and-Adapters style boundaries.

The Domain layer contains business rules and has no dependency on technical infrastructure.

The Application layer defines the ports it needs:

- use cases as inbound application ports
- command persistence ports
- query ports
- clock and domain-event dispatching ports

Infrastructure and API provide adapters for those ports.

```mermaid
flowchart LR
    Client[HTTP Client] --> Api[API Adapter<br/>Minimal API endpoints]

    Api --> UseCases[Application Use Cases<br/>Inbound Ports]

    UseCases --> Domain[Domain Model<br/>Aggregates, Value Objects,<br/>Domain Events, Domain Errors]

    UseCases --> UowPort[IUnitOfWork<br/>Command Port]
    UseCases --> QueryPort[IProductQueries<br/>Query Port]
    UseCases --> ClockPort[IClock<br/>Time Port]
    UseCases --> EventPort[IDomainEventDispatcher<br/>Domain Event Port]

    Infrastructure[Infrastructure Adapters] --> UowPort
    Infrastructure --> QueryPort
    Infrastructure --> ClockPort
    Infrastructure --> EventPort

    Infrastructure --> EfCore[EF Core DbContext<br/>DbSet, LINQ, Migrations]
    EfCore --> Database[(Relational Database)]
```

The ports are intentionally pragmatic. They do not try to hide every EF Core detail. Instead, they keep use cases independent from direct `DbContext`, `DbSet<T>`, and `IQueryable<T>` usage while still allowing Infrastructure to use EF Core effectively.

## Application boundary

The Application layer defines use cases and the ports required to execute them.

Command use cases access aggregates through a small command persistence boundary:

```text
IUnitOfWork
  -> IRepository<TAggregate, TKey>
```

Query use cases access read models through named query ports, for example:

```text
IProductQueries
```

This creates a deliberate command/query split:

| Side | Application dependency | Infrastructure implementation |
| --- | --- | --- |
| Commands | `IUnitOfWork`, `IRepository<TAggregate, TKey>` | `ApplicationDbContext`, EF Core tracking |
| Queries | named query interfaces such as `IProductQueries` | EF Core LINQ, projections, `AsNoTracking()` |

The abstraction is intentionally thin and EF Core-friendly.

The template does not introduce:

- a generic CRUD repository framework
- a specification framework
- a separate read-only DbContext abstraction
- persistence abstractions pretending that EF Core could be swapped out without design work

The goal is clarity and dependency direction, not abstraction for its own sake.

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
