# DATABASE

> Database provider selection and generated provider-specific configuration.

## Provider selection

The provider is selected when the project is generated:

```bash
dotnet new cleanapi -n Company.Template --db PostgreSql
```

Valid provider values:

- `PostgreSql`
- `SqlServer`

The selected provider is written to configuration:

```json
{
    "Database": {
        "Provider": "__DB_PROVIDER__",
        "ConnectionStringName": "DefaultConnection"
    },
    "ConnectionStrings": {
        "DefaultConnection": "Set by Aspire AppHost. Replace this when running the API directly."
    }
}
```

## Provider-specific code

Provider-specific EF Core configuration is isolated in:

```text
src/Company.Template.Infrastructure/Persistence/Providers/
```

Only the selected provider is compiled into the generated project.

Provider selection is a generation-time choice, not a runtime provider switch.

The generated project contains only the EF Core provider configuration and Testcontainers setup for the selected provider.

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
