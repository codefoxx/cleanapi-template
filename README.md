# Codefox Clean API Template

This repository contains a `dotnet new` template for a production-oriented Clean Architecture Web API.

The outer repository is the template authoring container. The generated application template lives in `content/` and is packaged by `template-package/`.

## Repository structure

~~~text
content/
  The actual template content.
  This is what users get when they run dotnet new cleanapi.

template-package/
  The NuGet template package project.
  It packs the content folder as a dotnet new template.

scripts/
  Local template authoring and verification scripts.
~~~

## Template goals

The template is intended to be close to a production-ready starting point, while still being small enough to understand.

It demonstrates:

- Clean Architecture project separation
- DDD-inspired domain model
- strongly typed IDs
- explicit use cases
- Result and Option patterns
- EF Core without custom repository/unit-of-work wrappers
- provider-selectable relational persistence
- EF Core migrations through a one-shot migration service
- .NET Aspire local orchestration
- OpenTelemetry and structured logging
- use-case telemetry decorators
- Keycloak JWT bearer wiring
- Testcontainers-based integration tests
- central package management
- executable-documentation style tests

## Architecture decisions

### Domain

The `Domain` project references no other project.

It contains the domain model, value objects, strongly typed IDs, aggregate behavior, and domain events.

The domain layer must not reference:

- EF Core
- ASP.NET Core
- Keycloak
- Aspire
- OpenTelemetry
- Serilog
- infrastructure concerns

### Application

The `Application` project contains use cases and application-level abstractions.

Use cases implement one of these contracts:

~~~csharp
public interface IUseCase<in TRequest, TResult>
{
    Task<Result<TResult>> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IUseCase<in TRequest>
{
    Task<Result> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}
~~~

Endpoints depend on these interfaces, not on concrete use-case classes. This allows cross-cutting behavior such as logging, tracing, metrics, and timing to be applied through decorators.

Use cases are registered automatically using Scrutor.

### Persistence

The template deliberately avoids custom repository and unit-of-work abstractions.

Instead:

- `DbContext` is treated as the unit of work.
- `DbSet<TEntity>` is treated as the repository.
- Read queries use `IQueryable<T>` and `AsNoTracking()`.
- Feature-specific DbContext interfaces expose the DbSets and query roots needed by that feature.
- Query extension methods provide named, composable queries.

Example:

~~~csharp
Product? product = await _dbContext.ProductsForRead
    .WithId(productId)
    .SingleOrDefaultAsync(cancellationToken);
~~~

For writes:

~~~csharp
_dbContext.Products.Add(product);

await _dbContext.SaveChangesAsync(cancellationToken);
~~~

The database provider is still abstracted behind provider-specific EF Core configuration. The generated project compiles only the selected provider.

### Observability

The template uses OpenTelemetry and structured logging as production concerns.

Service defaults configure:

- OpenTelemetry logging
- OpenTelemetry tracing
- OpenTelemetry metrics
- ASP.NET Core instrumentation
- HTTP client instrumentation
- runtime instrumentation
- OTLP export when configured

Use cases are wrapped by telemetry decorators. The decorator records:

- use-case start
- use-case completion
- use-case failure
- duration
- unexpected exceptions
- cancellations
- OpenTelemetry spans
- OpenTelemetry metrics
- structured logs

The split is intentional:

~~~text
Logs
  Explain what the application decided and why.

Traces
  Show request, use-case, dependency, and database flow.

Metrics
  Show rates, counts, failures, and duration distributions.
~~~

Generic execution telemetry lives in:

~~~text
content/src/Company.Template.Application/Telemetry/
~~~

Feature-specific business logs should live with the feature.

### Migrations

The generated solution includes a `MigrationService` project.

It is an executable one-shot process that applies EF Core migrations and exits. Aspire starts it after the database and before the API.

Local startup order:

~~~text
database
  -> migration service
  -> api
~~~

For production deployments, migration bundles are preferred over running migrations from the API at startup.

## Template workflow

Use the helper script for local template authoring:

~~~bash
./scripts/template.sh --help
~~~

Available commands:

~~~text
pack       Build the template NuGet package.
install    Pack and install the template locally.
create     Create a test project from the installed template.
migrate    Create an EF Core migration in the test project.
build      Restore and build the generated test project.
test       Install, create, migrate and build the generated project.
all        Same as test.
clean      Remove generated test output.
~~~

Examples:

~~~bash
./scripts/template.sh pack
./scripts/template.sh install
./scripts/template.sh create --db PostgreSql
./scripts/template.sh test --db PostgreSql
./scripts/template.sh test --db SqlServer --name Acme.Orders
./scripts/template.sh test --db MySql
~~~

The default test project is generated under:

~~~text
/tmp/cleanapi-template-test/Acme.Products
~~~

The script verifies:

- template installation
- project creation
- unresolved template placeholders
- provider-specific file pruning
- migration service presence
- package restore
- initial EF Core migration generation
- absence of EF Core design-time `BuildHost-*` artifacts
- solution build

## Build the template package manually

~~~bash
dotnet pack ./template-package/Codefox.CleanApi.Template.csproj -c Release
~~~

## Install locally manually

~~~bash
dotnet new install ./template-package/bin/Release/Codefox.CleanApi.Template.0.1.0.nupkg --force
~~~

Or use:

~~~bash
./scripts/template.sh install
~~~

## Generate a new API manually

~~~bash
dotnet new cleanapi -n Acme.Products --db PostgreSql
~~~

Supported database provider values:

- `PostgreSql`
- `SqlServer`
- `MySql`

## Working with `content/`

The `content/` folder is a real .NET solution and should stay buildable.

After running template packaging/install scripts, some `bin/` and `obj/` folders inside `content/` may be deleted intentionally. If Rider or the CLI complains about missing `project.assets.json`, restore the content solution:

~~~bash
cd content
dotnet restore
~~~

or:

~~~bash
cd content
dotnet build
~~~

## Template packaging notes

The template package project should pack template content, not compile the generated application.

The important setting is that files from `content/` are packed as content:

~~~xml
<None Include="$(MSBuildThisFileDirectory)..\content\**\*"
      Pack="true"
      PackagePath="content\" />
~~~

Avoid adding generated application files as `Compile` items in the template package project unless there is a very specific reason. The generated solution should compile those files, not the package project.

## Development notes

The template currently keeps optional infrastructure such as Keycloak and pgAdmin in the generated solution and enables them through configuration.

This avoids half-working template parameters that remove files but leave stale solution or project references behind. Optional pruning can be added later, but only deliberately and with full template tests for each combination.

## Recommended validation before committing

~~~bash
./scripts/template.sh test --db PostgreSql
~~~

When changing provider-specific code, also run:

~~~bash
./scripts/template.sh test --db SqlServer
./scripts/template.sh test --db MySql
~~~

## Generated project README

The README delivered with the generated application lives in:

~~~text
content/README.md
~~~

This repository README describes template authoring. The generated README describes how to use the generated application.