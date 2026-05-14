# ASPIRE

> Local orchestration with .NET Aspire.

## Running locally

Run:

```bash
dotnet run --project src/Company.Template.AppHost
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

## Local development tools

Local development tools can be enabled in:

```text
src/Company.Template.AppHost/appsettings.json
```

Example:

```json
{
    "AppHost": {
        "StartPgAdmin": false,
        "StartKeycloak": false
    }
}
```

## Optional pgAdmin

pgAdmin is disabled by default.

Enable it in the AppHost configuration:

```json
{
    "AppHost": {
        "StartPgAdmin": true
    }
}
```

pgAdmin is intended as a local development tool only. It is not part of the application architecture.

## Keycloak orchestration

Keycloak can also be started by the AppHost when authentication is enabled for local development.

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
