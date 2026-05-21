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

### Refresh solution, folder, and file structure documentation

The root README should not deeply document the generated structure until the structure is stable enough.

A later documentation story should explain the generated solution shape, project responsibilities, and file layout in a way that is useful but not brittle.

### Improve documentation for adding and removing features

The feature composition model intentionally differs from many templates.

The documentation should explain:

- how to add a feature,
- how to remove the `Products` sample,
- how to add use cases and endpoints,
- how to add persistence for a new aggregate,
- when to touch composition code,
- when not to touch `Composition.Abstractions` or `Composition.AspNetCore`.

### Revisit health checks and root endpoint behavior

The template should have a small, predictable default surface.

The root endpoint, health endpoints, and Aspire/service-default behavior should be reviewed as template contracts instead of accidental startup details.

### Add architecture boundary tests

Potential architecture tests:

- Domain does not reference Application, Infrastructure, API, or Composition.
- Application does not reference Infrastructure or API.
- Infrastructure does not reference API.
- `Composition.Abstractions` stays ASP.NET Core-free.
- `Composition.AspNetCore` is the only composition project with ASP.NET Core web application concepts.
- `MigrationService` stays independent from composition projects.

### Consider stronger template materialization tests

The current CI validates template materialization. A later story can decide whether some checks should move from shell scripts into C# tests or a small dedicated validation project.

The goal is not to over-engineer the test setup. The goal is to make the materialization contract easier to maintain.

### Consider a second vertical slice architecture template

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
