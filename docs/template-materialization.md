# Template Materialization

## Principle

The template source may contain authoring mechanics.

Generated projects must not.

The `content` tree is both:

1. the source of the .NET template, and
2. a development workspace for evolving and testing the template itself.

Because of that, the raw template source sometimes needs mechanics that only exist to make template development practical. For example, it must be possible to work on provider-specific PostgreSQL and SQL Server code in the same repository without materializing a new project for every change.

Those mechanics are acceptable in the template source, but they must not leak into projects created from the template.

A generated project should look like a normal hand-written project. It should not expose provider suffixes, authoring-only MSBuild properties, conditional provider package references, or other template implementation details.

## Database provider materialization

The template currently supports database-provider selection through the `db` template option:

```bash
dotnet new cleanapi -n My.Api --db PostgreSql

dotnet new cleanapi -n My.Api --db SqlServer
```

Provider-specific source files stay explicit in the raw template source so they are easy to find and maintain:

```text
PostgreSqlAspireDatabase.cs
SqlServerAspireDatabase.cs
DatabaseProviderConfigurator.PostgreSql.cs
DatabaseProviderConfigurator.SqlServer.cs
TestDatabase.PostgreSql.cs
TestDatabase.SqlServer.cs
TestDatabaseServer.PostgreSql.cs
TestDatabaseServer.SqlServer.cs
```

During materialization, the selected provider files are renamed to neutral output names:

```text
PostgreSqlAspireDatabase.cs / SqlServerAspireDatabase.cs
  -> AspireDatabase.cs

DatabaseProviderConfigurator.PostgreSql.cs / DatabaseProviderConfigurator.SqlServer.cs
  -> DatabaseProviderConfigurator.cs

TestDatabase.PostgreSql.cs / TestDatabase.SqlServer.cs
  -> TestDatabaseProvider.cs

TestDatabaseServer.PostgreSql.cs / TestDatabaseServer.SqlServer.cs
  -> TestDatabaseServer.cs
```

This is implemented with conditional `sources` entries in `.template_config/template.json`.

The default source excludes all provider-specific implementation files. Provider-specific source entries then include only the selected provider files and materialize them under neutral names.

## Why `TestDatabaseProvider.cs` exists

`TestDatabase.cs` is already the provider-neutral test database abstraction. It contains the shared test database behavior and the partial method declaration used by provider-specific implementations.

For that reason, provider-specific files must not materialize as `TestDatabase.cs`.

They materialize as:

```text
TestDatabaseProvider.cs
```

This keeps both pieces in the generated project:

```text
TestDatabase.cs          // provider-neutral shared behavior
TestDatabaseProvider.cs  // selected provider-specific partial implementation
```

## Package references

Provider-specific package versions and package references are selected at template time.

The raw template source may contain XML-comment template conditionals such as:

```xml
<!--#if (db == "PostgreSql") -->
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0"/>
<!--#endif -->
<!--#if (db == "SqlServer") -->
<PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0"/>
<!--#endif -->
```

Generated projects should contain only the selected provider package references.

A generated PostgreSQL project must not contain SQL Server provider packages such as:

```text
Aspire.Hosting.SqlServer
Microsoft.Data.SqlClient
Microsoft.EntityFrameworkCore.SqlServer
Testcontainers.MsSql
```

A generated SQL Server project must not contain PostgreSQL provider packages such as:

```text
Aspire.Hosting.PostgreSQL
Npgsql
Npgsql.EntityFrameworkCore.PostgreSQL
Testcontainers.PostgreSql
```

## Authoring-only MSBuild logic

The raw template source can use `EffectiveDbProvider` for direct development of the `content` tree.

This allows template contributors to build and test the raw template source without first generating a new project.

Generated projects do not need `EffectiveDbProvider`. They already contain only the selected provider files and selected provider package references.

For this reason, authoring-only MSBuild logic is wrapped in template-time false blocks:

```xml
<!--#if (db == "TemplateAuthoringOnly") -->
<ItemGroup Condition="'$(EffectiveDbProvider)' != 'PostgreSql'">
    <Compile Remove="...PostgreSql.cs"/>
</ItemGroup>
<!--#endif -->
```

The `TemplateAuthoringOnly` value is intentionally not a public template choice. The block is kept in the raw source for template development, but is removed from generated projects.

Generated `.csproj` and `.props` files must not contain:

```text
EffectiveDbProvider
Condition=.*PostgreSql
Condition=.*SqlServer
```

## Conditional block formatting

Template conditionals should be placed with generated whitespace in mind.

The .NET Template Engine removes conditional directives and excluded lines, but it does not reformat the file afterwards. Poorly placed conditionals can therefore leave behind excessive blank lines in generated files.

Use the largest conditional block that keeps the generated file natural.

Split conditionals only when grouping would make the generated file less readable, less sorted, or less idiomatic.

The generated project has priority over the raw template source.

## Validation

There are two validation layers.

### Local developer validation

Run:

```bash
bash scripts/validate-template-materialization.sh
```

The script materializes both supported providers and validates provider-clean output.

It checks for:

```text
- no provider-specific filename suffixes
- expected neutral provider files
- no EffectiveDbProvider in generated project files
- no provider MSBuild conditions in generated project files
- no wrong-provider package references
```

This script is a developer convenience.

### Pipeline validation

The GitHub Actions workflow `.github/workflows/template-materialization.yml` performs the same category of checks inline.

The workflow intentionally does not call the local validation script.

The pipeline is its own authoritative gate. A local script change should not silently change CI behavior.

## Out of scope

This materialization story does not solve unrelated build or composition API issues.

In particular, the current nullable diagnostic around `params Assembly[]` / extension members in the Composition startup path is treated as a separate code-quality topic.

That issue should be handled in a dedicated cleanup story, not mixed into template materialization.
