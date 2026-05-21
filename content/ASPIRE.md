# ASPIRE

> Local orchestration with .NET Aspire.

## Running locally

Run:

```bash
dotnet run --project src/aspire/Company.Template.AppHost
```

Aspire starts the selected database container, runs the migration service, and then starts the API.

The local startup order is:

```text
database
  -> migration service
  -> api
```

The migration service applies pending EF Core migrations and exits.

The API waits until the migration service has completed successfully.

## AppHost configuration

The AppHost is the local orchestration entry point for generated projects.

Database resources are selected through the template's database provider option and materialized into provider-specific AppHost code.

Authentication-related orchestration is selected through the template's authentication option.

## Keycloak orchestration

When the project is generated with Keycloak authentication, AppHost starts the sample Keycloak setup used by the local API.

See [AUTHENTICATION.md](AUTHENTICATION.md) for the full Keycloak setup.

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