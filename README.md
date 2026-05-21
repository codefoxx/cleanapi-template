# cleanapi-template

> Production-oriented .NET Web API templates packaged for `dotnet new`.

This repository currently contains the Clean Architecture template in `content/`.

The template is still evolving, but the goal is stable: provide a realistic .NET Web API starting point with Clean Architecture, pragmatic Ports-and-Adapters boundaries, DDD-style domain modeling, explicit expected-failure handling, EF Core persistence, Aspire orchestration, Minimal APIs, and optional authentication materialization.

The documentation in `content/` is copied into generated projects. Repository planning notes live under `docs/`.

## Quick start

Create a project from the installed template:

```bash
dotnet new cleanapi -n MyCompany.Products --db PostgreSql --auth Keycloak
```

Available template options:

```text
--db    PostgreSql | SqlServer   (default: PostgreSql)
--auth  None | Keycloak          (default: Keycloak)
```

Restore packages:

```bash
dotnet restore
```

Create the initial migration:

```bash
dotnet ef migrations add InitialCreate \
  --project src/MyCompany.Products.Infrastructure \
  --startup-project src/MyCompany.Products.CompositionRoot \
  --context ApplicationDbContext \
  --output-dir Persistence/Migrations
```

Run the local Aspire AppHost:

```bash
dotnet run --project src/aspire/MyCompany.Products.AppHost
```

Run tests:

```bash
dotnet test
```

## Project status and roadmap

### Completed

- ✅ Clean Architecture template baseline
- ✅ EF Core-backed persistence boundary
- ✅ PostgreSQL and SQL Server generation-time provider materialization
- ✅ Optional authentication materialization with `--auth None` and `--auth Keycloak`
- ✅ Keycloak AppHost integration for local development
- ✅ Result / Option based expected-failure flow
- ✅ Feature-oriented composition model
- ✅ Simplified generated solution structure
- ✅ `CompositionRoot` executable entry point naming
- ✅ Composition and Aspire support project grouping
- ✅ Feature catalog split by ownership
- ✅ Template materialization validation in CI
- ✅ Root repository documentation and generated project documentation split

### Intentionally not included

- 🚫 pgAdmin container support
  - Removed intentionally to keep the Aspire setup focused and avoid template bloat.
- 🚫 Runtime database provider switching
  - Provider selection is a generation-time template choice.
- 🚫 Runtime authentication on/off switches
  - Authentication is a generation-time template choice.

### Planned / under consideration

#### Application model

- ⬜ Make command/query handler and decorator model explicit
- ⬜ Keep the MediatR-style pipeline small and dependency-free

#### Production behavior

- ⬜ Add transactional outbox baseline for domain events
- ⬜ Expand structured logging and telemetry
- ⬜ Revisit EF Core persistence and read-model options
- ⬜ Harden configuration, API security, and API versioning decisions

#### Template quality and documentation

- ⬜ Add generated-template contract tests
- ⬜ Improve generated project first-use experience
- ⬜ Make the `Products` sample easy to remove
- ⬜ Introduce lightweight ADRs and production deployment notes

#### Future template family

- ⬜ Consider production containerization support
- ⬜ Consider a second Vertical Slice Architecture template later

For more detail, see [docs/ROADMAP.md](docs/ROADMAP.md).

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
| [content/ASPIRE.md](content/ASPIRE.md) | Local orchestration and startup order |
| [content/AUTHENTICATION.md](content/AUTHENTICATION.md) | Optional Keycloak authentication and local realm setup |
| [content/OPENAPI.md](content/OPENAPI.md) | OpenAPI document, response metadata, OAuth metadata |
| [content/MIGRATIONS.md](content/MIGRATIONS.md) | EF Core migrations and production migration bundles |
| [content/TESTING.md](content/TESTING.md) | Unit, integration, Testcontainers, API problem tests, smoke tests |
| [content/FEATURES.md](content/FEATURES.md) | How to add a new feature to the template |
| [content/docs/FEATURE_COMPOSITION.md](content/docs/FEATURE_COMPOSITION.md) | Feature composition, markers, decorators, and web pipeline composition |

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

## Central package management

Package versions are centralized in:

```text
Directory.Packages.props
```

Project files reference packages without versions.
