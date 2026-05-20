# Improve Template Materialization

## Branch

```text
story/improve-template-materialization
```

## Goal

Improve how the CleanAPI template materializes projects from `content/.template_config/template.json`.

The generated project should look like a normal hand-written project. It should not expose template-authoring mechanics such as provider-specific filename suffixes, MSBuild option conditions, unused authentication files, or unresolved template tokens.

At the same time, the template source should remain practical to work on directly. While developing the template, it should still be possible to build and test the `content` tree without first materializing a generated project for every option combination.

## Guiding principle

```text
Template source may contain authoring mechanics.
Generated projects must not.
```

## In scope

- Investigate .NET Template Engine capabilities inside a disposable spike branch.
- Decide how to use `symbols`, `sources`, `modifiers`, `exclude`, `rename`, `fileRename`, and conditional content.
- Keep or replace `EffectiveDbProvider` based on what the Template Engine can support cleanly.
- Materialize selected database provider files with neutral generated filenames.
- Exclude unselected database provider files from generated projects.
- Remove provider-option MSBuild conditions from generated projects.
- Add or prepare an authentication selector:
  - `None`
  - `Keycloak`
- Ensure Keycloak/authentication files do not leak into `auth=None` generated projects.
- Extend template validation scripts to detect materialization leftovers.

## Out of scope

- Core `Result` / `Option` / `Validation` behavior tests from the architecture review findings.
- Architecture tests.
- Feature Composition redesign.
- Deep `Products` sample business testing.
- Vertical Slice template work.
- Keycloak multi-tenant or realm lifecycle complexity.

## Disposable spike branch

```text
spike/template-engine-materialization
```

The spike branch exists to test Template Engine behavior without polluting the final story history.

It may contain temporary files, experimental `template.json` changes, and rough commits.

Do not merge the spike branch as-is.

After the spike, either:

1. discard the spike and implement the clean solution on the story branch, or
2. cherry-pick only clean, useful commits into the story branch.

## Questions the spike must answer

1. Can conditional source definitions rename selected files to neutral output names?
2. Can `sources.rename` or another mechanism handle cases such as:

   ```text
   DatabaseProviderConfigurator.PostgreSql.cs -> DatabaseProviderConfigurator.cs
   DatabaseProviderConfigurator.SqlServer.cs  -> DatabaseProviderConfigurator.cs
   ```

3. Can `.csproj` / `.props` files use template-time conditional content so the generated project contains only the selected provider package references?
4. Can the template source remain directly buildable/testable without leaking `EffectiveDbProvider` or equivalent authoring properties into generated projects?
5. Can the same mechanism later support `auth=None|Keycloak` cleanly?

## Expected generated PostgreSQL output

A generated PostgreSQL project should contain provider files such as:

```text
DatabaseProviderConfigurator.cs
TestDatabase.cs
TestDatabaseServer.cs
AspireDatabase.cs
```

It should not contain:

```text
*.PostgreSql.cs
*.SqlServer.cs
EffectiveDbProvider
Condition="'$(EffectiveDbProvider)' ..."
```

## Expected generated SQL Server output

A generated SQL Server project should follow the same pattern:

```text
DatabaseProviderConfigurator.cs
TestDatabase.cs
TestDatabaseServer.cs
AspireDatabase.cs
```

No provider-specific filename suffixes or template-authoring MSBuild conditions should remain.

## Expected generated auth output

For `auth=None`:

- no Keycloak container wiring
- no Keycloak realm files
- no JwtBearer package reference unless needed for another reason
- no auth policy leftovers
- no unresolved auth tokens

For `auth=Keycloak`:

- Keycloak files are present as normal project files
- no unresolved Keycloak tokens remain
- the project does not look like it contains optional-auth leftovers

## Validation direction

The final story should extend generated-template validation so it can detect:

```bash
find . -name "*.PostgreSql.cs" -o -name "*.SqlServer.cs"
grep -R "EffectiveDbProvider\|Condition=.*PostgreSql\|Condition=.*SqlServer" . --include="*.csproj" --include="*.props"
grep -R "__DB_PROVIDER__\|__KEYCLOAK_REALM__\|__AUTH_AUDIENCE__" .
```

For `auth=None`, validation should also fail on meaningful Keycloak/auth leftovers.

## Suggested final commit shape

```text
refactor: materialize selected database provider files
refactor: remove provider conditions from generated projects
feat: add authentication template option
test: validate materialized template output
docs: document template materialization options
```
