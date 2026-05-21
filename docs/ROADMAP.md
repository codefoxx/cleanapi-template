# Roadmap

This roadmap describes the repository direction at a practical level.

It is not a release plan and it is not meant to document every past story. The root README should stay short. This file can hold the longer notes that help decide what to do next.

## Current focus

Refresh the repository documentation after the provider and authentication materialization work.

The root README should explain the current state without over-documenting unstable generated solution structure. Deeper generated-project documentation stays under `content/` because it is copied into new projects.

## Completed

- ✅ Initial Clean Architecture template baseline.
- ✅ Clean Architecture / Ports and Adapters style layering.
- ✅ Minimal API HTTP adapter.
- ✅ EF Core-backed persistence boundary.
- ✅ PostgreSQL and SQL Server provider materialization with `--db PostgreSql` and `--db SqlServer`.
- ✅ Optional authentication materialization with `--auth None` and `--auth Keycloak`.
- ✅ Keycloak AppHost integration for local development.
- ✅ Result / Option based expected-failure flow.
- ✅ Lightweight API request validation helper.
- ✅ Feature-oriented composition model.
- ✅ Template materialization validation in CI.
- ✅ Root repository documentation and generated project documentation split.

## Intentionally not included

- 🚫 pgAdmin container support.

  Removed intentionally to keep the Aspire setup focused and avoid growing the base template into a local tooling bundle.

- 🚫 Runtime database provider switching.

  Database provider selection is a template materialization choice. A generated project should contain the selected provider implementation, not a runtime abstraction that pretends every provider is interchangeable.

- 🚫 Runtime authentication on/off switches.

  Authentication is a template materialization choice. `--auth None` should produce a clean no-auth project, while `--auth Keycloak` should produce a Keycloak-enabled project. Old runtime switches such as `Authentication:Enabled`, `StartKeycloak`, and `KeycloakUseDataVolume` should not return.

## Planned / under consideration

### Foundation and generated solution structure

#### Simplify generated solution structure

The generated solution currently contains many projects at the same visual level. That makes the architecture harder to understand than it needs to be.

A later story should review whether generated projects can be grouped so the core Clean Architecture projects stay visually prominent:

- `Api`
- `Application`
- `Domain`
- `Infrastructure`
- `CompositionRoot`

Composition-related mechanics should move under a dedicated composition folder, and Aspire/local-development projects should move under a dedicated Aspire folder.

Possible target shape:

```text
src/
  Company.Template.Api/
  Company.Template.Application/
  Company.Template.Domain/
  Company.Template.Infrastructure/
  Company.Template.CompositionRoot/

src/composition/
  Company.Template.Composition.Abstractions/
  Company.Template.Composition.AspNetCore/

src/aspire/
  Company.Template.AppHost/
  Company.Template.ServiceDefaults/
  Company.Template.MigrationService/
```

#### Rename the executable Composition project to CompositionRoot

The current `Composition` project is the executable ASP.NET Core entry point. The name is too close to the reusable composition mechanism.

The preferred direction is:

```text
Company.Template.Composition
  -> Company.Template.CompositionRoot
```

`CompositionRoot` should mean:

- executable ASP.NET Core entry point,
- application startup and module activation,
- the place that references API, Application, Domain, and Infrastructure,
- not the reusable composition mechanism itself.

#### Split feature catalog by ownership

The feature catalog should not live in the reusable composition abstraction package.

When the generated solution structure is simplified, feature markers should move to the assemblies that own the corresponding concern:

- API-owned HTTP/OpenAPI/endpoint features live in `Api`.
- Application-owned handler/decorator features live in `Application`.
- Infrastructure-owned persistence/event features live in `Infrastructure`.
- Composition-root-only defaults live in `CompositionRoot`.
- Domain should stay free of composition feature markers.

This keeps `Composition.Abstractions` generic while allowing the executable `CompositionRoot` project to reference all application layers and activate the desired features.

### Application model

#### Make the handler/decorator model explicit

The current `IUseCase<TRequest, TResult>` abstraction is close to a small MediatR-style request handler model built with standard .NET dependency injection.

A later story should evaluate renaming the application use-case interfaces and implementations to make that intent clearer:

- commands handled by `ICommandHandler<...>`
- queries handled by `IQueryHandler<...>`
- use-case decorators described as handler decorators / pipeline behavior equivalents
- no dependency on MediatR required
- no hidden mediator dispatch layer unless it adds clear value

The goal is not to clone MediatR. The goal is to show that modern .NET and DI are enough for this template's command/query handler pipeline.

### Production behavior

#### Add transactional outbox for domain events

The current template dispatches recorded domain events after `SaveChangesAsync`.

That keeps the sample simple, but it can lose side effects when persistence succeeds and event dispatch fails.

A later story should evaluate moving domain event persistence into a transactional outbox:

- use EF Core `SaveChangesInterceptor`
- collect recorded aggregate domain events before saving changes
- write outbox rows in the same database transaction as aggregate changes
- store a stable event type and serialized JSON payload
- use PostgreSQL `jsonb` for the payload
- use SQL Server string storage, for example `nvarchar(max)`, for the payload
- keep telemetry/correlation context inside the serialized event envelope by default
- clear domain events only after successful save
- process outbox messages asynchronously
- start with a simple processor that logs domain events as structured logs
- mark messages as processed or failed
- document retry and multi-instance limitations
- keep external messaging out of the base template unless it clearly earns its place

The baseline outbox message should stay focused on storage and processing metadata:

```csharp
public sealed class OutboxMessage
{
    public Guid Id { get; private init; }
    public string Type { get; private init; } = string.Empty;
    public string Payload { get; private init; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private init; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset? LastAttemptedAtUtc { get; private set; }
    public DateTimeOffset? NextAttemptAtUtc { get; private set; }
    public string? Error { get; private set; }
}
```

The goal is not to implement a full messaging platform. The goal is to show a production-oriented default that does not lose domain events after a successful database commit.

#### Expand structured logging and telemetry

Observability should become a first-class production concern in the template.

The goal is to show how a generated API can be debugged in production through structured logs, traces, and clear diagnostic boundaries.

Potential scope:

- define structured logging conventions
- avoid interpolated log messages
- add command/query handler telemetry decorators
- distinguish expected `Result` failures from unexpected exceptions
- enrich logs with trace/correlation information
- include API version information in logs once API versioning is introduced
- make global exception handling diagnostics explicit
- add useful Activity names and tags for handlers and persistence boundaries
- review EF Core logging defaults for development vs production
- document what should and should not be logged
- avoid leaking PII or sensitive authentication data
- keep Domain free of logging dependencies

The template should not log everything. It should log important boundary events with stable names and useful structured properties.

#### Revisit EF Core persistence model

The template already keeps EF Core behind Infrastructure, but the EF Core side can be modeled more deliberately.

A later spike/story should explore whether the template should use more EF Core capabilities to make persistence clearer and more production-oriented:

- `IEntityTypeConfiguration<T>` per aggregate/read model
- clearer write-model mappings
- a dedicated read-only DbContext
- specialized read models with navigation properties
- query definitions or query helper patterns inside Infrastructure
- keeping Application query ports while improving EF-side implementation

The goal is not to hide EF Core. The goal is to use EF Core well without leaking it into Application use cases.

#### Harden configuration and options validation

Configuration should have clear ownership and should fail fast when required values are missing or invalid.

A later story should review:

- which settings belong to `CompositionRoot`, `Api`, `Infrastructure`, `AppHost`, or generated tests
- which settings are development-only
- which settings are production-relevant
- whether options use `ValidateOnStart()` where useful
- whether auth, database, telemetry, and integration settings have clear validation
- whether generated no-auth and Keycloak projects contain only the settings they need

#### Review API security defaults

Authentication is only one part of production API security.

A later story should review safe defaults and documentation for:

- CORS stance
- secure response headers
- problem response leakage
- PII logging policy
- request size limits
- rate limiting as an optional/default decision
- authorization policy examples
- differences between `--auth None` and `--auth Keycloak`

The goal is not to add an enterprise security framework. The goal is to avoid unsafe or misleading defaults.

#### Decide API versioning strategy

API versioning should be a conscious template decision, not an accidental omission.

URL-based versioning is simple and common, but it makes routes noisy. The preferred direction is to explore header-based API versioning because it keeps resource URLs cleaner.

If header-based versioning is used, the version header must become part of diagnostics:

- request logs should include the API version
- traces should include the API version as a tag where useful
- problem logs should make version mismatches visible
- OpenAPI documentation must make the required/default version behavior clear
- tests should verify missing, unsupported, and supported version behavior

The goal is to keep the public API clean without making production diagnostics worse.

### Template quality

#### Add generated-template contract tests

The CI already validates template materialization, but later work should make generated-template expectations more explicit.

Potential checks:

- generated project contains expected files
- generated project does not contain excluded auth/provider files
- generated project contains expected package references
- generated project has no unresolved template tokens
- generated project has expected startup project
- generated project has expected documentation
- generated project has expected project references

These checks may eventually move from shell scripts into C# tests or a small dedicated validation project.

#### Improve generated project first-use experience

After `dotnet new`, the generated project should feel clean and immediately understandable.

A later story should review:

- whether the generated README points to the right first commands
- whether the sample can be understood quickly
- whether there are any template leftovers
- whether the startup project is obvious
- whether the project works well after `git init`
- whether a new developer can run tests and the AppHost without hidden steps

#### Make the Products sample easy to remove

The sample `Products` feature exists to prove the template wiring. It should not become hard to remove.

A later story should verify and document how to remove the sample feature cleanly:

- endpoints
- handlers/use cases
- domain model
- persistence mappings
- tests
- feature composition entries
- documentation references

This is important because a generated template should not trap real projects inside the sample domain.

#### Review dependency footprint

The generated project should keep dependencies intentional.

A later story should review:

- which dependencies are needed by production projects
- which dependencies belong only to tests
- which dependencies belong only to Aspire/local development
- whether support packages leak into the core application unnecessarily
- whether a dependency can be replaced by a small explicit pattern without hurting clarity

### Documentation

#### Refresh solution, folder, and file structure documentation

The root README should not deeply document the generated structure until the structure is stable enough.

A later documentation story should explain the generated solution shape, project responsibilities, and file layout in a way that is useful but not brittle.

#### Improve documentation for adding and removing features

The feature composition model intentionally differs from many templates.

The documentation should explain:

- how to add a feature,
- how to remove the `Products` sample,
- how to add use cases and endpoints,
- how to add persistence for a new aggregate,
- when to touch composition code,
- when not to touch `Composition.Abstractions` or `Composition.AspNetCore`.

#### Introduce lightweight ADRs

The repository has several important design decisions that deserve durable documentation.

Potential ADRs:

- use Clean Architecture with pragmatic Ports and Adapters
- use explicit `Result` / `Option` models for expected failures
- avoid MediatR as a default dependency
- use EF Core as an explicit Infrastructure adapter
- keep composition mechanics as source instead of a NuGet black box
- use a transactional outbox for domain events

ADRs should stay short and useful. They should explain decisions and trade-offs, not become essays.

#### Document error code conventions

The template has explicit error types and machine-readable error codes.

A later story should document:

- naming conventions for error codes
- domain error vs application error responsibilities
- validation field errors vs top-level problem code
- when error codes are stable external contracts
- how HTTP status is derived from `ErrorType`, not from the code string

#### Add production deployment notes

Aspire supports local development and orchestration. Production deployment should be documented separately.

Potential scope:

- API process/container
- database configuration
- migration bundles
- environment variables
- auth provider configuration
- OpenTelemetry exporter configuration
- logging sink configuration
- what AppHost is and is not meant for

This should stay infrastructure-neutral unless a specific provider clearly earns its place.

### Future template family

#### Consider production containerization support

A later story can evaluate whether the generated project should include production containerization support.

Open questions:

- API Dockerfile or SDK-generated container image?
- non-root runtime user
- health check endpoint expectations
- migration bundle workflow
- interaction with Aspire local development
- whether this belongs in the base template or optional documentation

#### Consider a second vertical slice architecture template

The package may later contain a second template based on Vertical Slice Architecture.

That should happen after the Clean Architecture template is stable enough. The second template should share the production-oriented baseline where it makes sense, but it should not pretend to be the same architecture with different folders.

## Open documentation ideas

Wiki pages, gists, ADRs, or longer design notes may be useful for explaining ideas that are too essay-like for the README.

For now, repository documentation should remain the source of truth. External notes should not be required to understand or use the template.

Possible future design notes:

- why this template uses Ports and Adapters pragmatically,
- why EF Core is not hidden behind fake database neutrality,
- why expected failures use Result / Option types,
- how feature composition evolved,
- how the future Clean Architecture and Vertical Slice templates should differ.

## Documentation rule of thumb

- Root README: short entry point.
- `docs/ROADMAP.md`: repository roadmap and planning notes.
- `content/*.md`: documentation copied into generated projects.
- Possible future ADRs: durable architectural decisions.
- Possible wiki/gists: optional essays, not required project documentation.
