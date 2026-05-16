# TESTING

> Tests should act as executable documentation.

## Running tests

Run:

```bash
dotnet test
```

Integration tests use Testcontainers and the selected relational database provider, currently PostgreSQL or SQL Server.

Do not use EF Core InMemory as a substitute for relational integration tests.

## Test style

Tests follow an Arrange / Act / Assert structure.

Use descriptive names such as:

```text
CreateProduct_WhenRequestIsValid_ShouldCreateProduct
GetProductById_WhenProductExists_ShouldReturnProduct
SaveChanges_WhenProductIsAdded_ShouldPersistAndReloadProduct
```

The goal is that tests explain the behavior they protect.

## Layered test intent

| Test project | Main purpose |
| --- | --- |
| `Domain.Tests` | Business rules, value objects, domain events, strict and safe creation APIs |
| `Application.Tests` | Use case behavior, result handling, query behavior |
| `Infrastructure.Tests` | EF Core mapping, persistence, provider behavior |
| `Api.Tests` | HTTP boundary, status codes, endpoint behavior, authentication/authorization |

## Failure model tests

For domain creation APIs, test both sides of the policy:

```text
Create / From
  strict API
  throws for contract violations

TryCreate / TryFrom
  safe API
  returns false and DomainError for expected validation failures
```

For application use cases, expected failures should be asserted as `Result<T>.Failure(...)`.

Do not assert expected validation behavior by catching exceptions from use cases.


## Database-backed tests

Database-backed tests use the shared `Company.Template.TestSupport` project.

The expensive database server container is owned by `TestDatabaseServer` and is shared per test assembly.
Individual tests create isolated logical databases with `TestDatabase`. This keeps tests deterministic without
starting one PostgreSQL or SQL Server container per test class or test method.

```text
Test assembly
└── one shared database server container
    └── isolated logical databases for tests
```

Use this pattern for application, infrastructure, and API tests that need real relational behavior.
Avoid EF Core InMemory for persistence tests because it does not exercise provider mappings, relational constraints,
or SQL translation.

## API test factories

API tests distinguish between lightweight and database-backed hosts:

- `ApiLightweightTestFactory` starts the API host without a database provider. Use it for OpenAPI, root endpoint,
  or HTTP-pipeline tests that must not touch persistence.
- `ApiDatabaseTestFactory` is used through `ApiTestContext` for endpoint behavior tests that write or query data.

This keeps metadata-style tests cheap while preserving full persistence coverage where it matters.

## Smoke tests

Keycloak + API smoke tests are documented in [AUTHENTICATION.md](AUTHENTICATION.md).

The smoke test validates the local authentication setup and sample Product endpoints end-to-end.

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
