# MIGRATIONS

> EF Core migrations for relational schema management.

## Overview

The template uses EF Core migrations for relational schema management.

The `MigrationService` project is used for local Aspire development. It applies pending migrations before the composition entry point starts.

## Creating the initial migration

After generating a project, create the initial migration once:

```bash
dotnet ef migrations add InitialCreate \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Composition \
  --context ApplicationDbContext \
  --output-dir Persistence/Migrations
```

The migration files are created in:

```text
src/Company.Template.Infrastructure/Persistence/Migrations/
```

Creating migration files does not require the database container to be running.

EF Core only needs to:

- build the project
- create the DbContext at design time
- compare the current model with the model snapshot

After the initial migration exists, run the AppHost:

```bash
dotnet run --project src/aspire/Company.Template.AppHost
```

The migration service will apply the migration automatically during Aspire startup.

## Adding later migrations

After changing the EF Core model, add a new migration:

```bash
dotnet ef migrations add DescribeYourChange \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Composition \
  --context ApplicationDbContext \
  --output-dir Persistence/Migrations
```

Then restart the AppHost.

The migration service applies the pending migration.

## Applying migrations manually

You can also apply migrations manually:

```bash
dotnet ef database update \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Composition \
  --context ApplicationDbContext
```

When running with Aspire locally, manual migration execution is usually not needed because the migration service handles it.

## Migration bundles

For release pipelines, prefer EF Core migration bundles over running migrations from the API process at startup.

Create a migration bundle:

```bash
dotnet ef migrations bundle \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Composition \
  --context ApplicationDbContext \
  --output artifacts/efbundle
```

For a Linux self-contained bundle:

```bash
dotnet ef migrations bundle \
  --project src/Company.Template.Infrastructure \
  --startup-project src/Company.Template.Composition \
  --context ApplicationDbContext \
  --self-contained \
  --runtime linux-x64 \
  --output artifacts/efbundle
```

Run the bundle with a deployment connection string:

```bash
./artifacts/efbundle --connection "$CONNECTION_STRING"
```

## Recommended production flow

```text
build application
build migration bundle
apply migration bundle to database
deploy or start composition API process
```

The migration service is mainly intended for local Aspire development.

A release pipeline should apply migrations explicitly before the API process is deployed or started.

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
