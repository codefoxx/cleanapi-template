# Story — Add Auth Template Switch

## Story branch

```text
story/add-auth-template-switch
```

## Goal

Add authentication as a template materialization option.

The generated project should support:

```bash
dotnet new cleanapi -n My.Api --auth None

dotnet new cleanapi -n My.Api --auth Keycloak
```

The generated output should look like a normal project for the selected authentication mode.

A no-auth generated project must not look like a Keycloak project with Keycloak merely disabled at runtime.

A Keycloak generated project should keep the current Keycloak-enabled behavior.

## Why this story exists

Authentication is currently controlled mainly through runtime configuration.

That is useful while developing the template, but it is not enough for generated projects.

If a user chooses a no-auth project, the generated output should not contain unnecessary Keycloak packages, containers, realm files, or Keycloak-specific settings.

This story applies the same materialization discipline already used for database providers to authentication.

## In scope

- Add an `auth` template choice.
- Support `None` and `Keycloak` as initial auth modes.
- Materialize auth-specific package references.
- Materialize auth-specific files.
- Materialize auth-specific AppHost resources.
- Materialize auth-specific `appsettings` content.
- Keep raw template development practical.
- Extend local template validation for the auth matrix.
- Extend pipeline template validation for the auth matrix.
- Document the auth template option.

## Out of scope

- Multi-tenant identity design.
- Enterprise identity-provider federation.
- Multiple external auth providers.
- Full Keycloak realm lifecycle management.
- Reworking the feature composition API.
- Fixing unrelated nullable Composition diagnostics.
- Deep Product-domain test expansion.

## Initial auth options

```text
None
Keycloak
```

### `None`

Generated project should not contain Keycloak-specific template leftovers.

Target output:

```text
no Keycloak package references
no Keycloak AppHost resource
no Keycloak container extension files
no Keycloak realm/import files
no Keycloak-specific appsettings sections
no JWT bearer authority/audience configuration unless required by neutral runtime code
no auth-specific generated leftovers
```

Prefer removing auth configuration entirely for `None` if the generated project can work without an explicit disabled setting.

If the runtime code requires an explicit setting, keep only the smallest neutral shape, for example:

```json
{
  "Authentication": {
    "Enabled": false
  }
}
```

### `Keycloak`

Generated project should contain the Keycloak-enabled setup.

Target output:

```text
Keycloak package references
Keycloak AppHost resource
Keycloak container extension files
Keycloak realm/import files
auth-related appsettings sections
JWT bearer/auth wiring
OpenAPI auth metadata where applicable
```

## Current-state inventory from `main`

The current `main` snapshot contains auth-related content in several areas.

### Runtime configuration

```text
content/src/Company.Template.AppHost/appsettings.json
content/src/Company.Template.AppHost/appsettings.Development.json
content/src/Company.Template.CompositionRoot/appsettings.json
content/src/Company.Template.CompositionRoot/appsettings.Development.json
content/src/Company.Template.MigrationService/appsettings.json
content/src/Company.Template.MigrationService/appsettings.Development.json
```

Auth is currently switched through runtime configuration, so `appsettings` must be treated as first-class story scope.

### API auth and OpenAPI wiring

```text
content/src/Company.Template.Api/ApiAdapterServiceModule.cs
content/src/Company.Template.Api/ApiAdapterWebAppModule.cs
content/src/Company.Template.Api/ApiCrossCuttingServiceModule.cs
content/src/Company.Template.Api/ApiCrossCuttingWebAppModule.cs
content/src/Company.Template.Api/Company.Template.Api.csproj
content/src/Company.Template.Api/Endpoints/Products/ProductEndpoints.cs
content/src/Company.Template.Api/OpenApi/AuthenticationOpenApiTransformers.cs
content/src/Company.Template.Api/OpenApi/AuthorizationOperationTransformer.cs
content/src/Company.Template.Api/OpenApi/BearerSecuritySchemeTransformer.cs
content/src/Company.Template.Api/OpenApi/OAuth2SecuritySchemeTransformer.cs
content/src/Company.Template.Api/OpenApi/OpenApiExtensions.cs
content/src/Company.Template.Api/Options/AuthenticationOptions.cs
content/src/Company.Template.Api/Security/AuthenticationExtensions.cs
```

### AppHost and Keycloak container wiring

```text
content/src/Company.Template.AppHost/AppHostNames.cs
content/src/Company.Template.AppHost/Company.Template.AppHost.csproj
content/src/Company.Template.AppHost/Containers/KeycloakContainerExtensions.cs
content/src/Company.Template.AppHost/Containers/KeycloakContainerOptions.cs
content/src/Company.Template.AppHost/Program.cs
content/src/Company.Template.AppHost/appsettings.json
```

### Keycloak realm files

```text
content/infra/keycloak/realms/__KEYCLOAK_REALM__-realm.json
```

### Template/package scripts and metadata

```text
content/.template_config/template.json
content/Directory.Packages.props
scripts/template.sh
template-package/Codefox.CleanApi.Template.csproj
.github/workflows/ci.yml
.github/workflows/template-materialization.yml
```

### Documentation likely affected

```text
README.md
content/README.md
content/API.md
content/ARCHITECTURE.md
content/ASPIRE.md
content/AUTHENTICATION.md
content/FEATURES.md
content/OPENAPI.md
content/TESTING.md
```

## Validation matrix

At the end of the story, template materialization should validate at least:

```text
PostgreSql + None
PostgreSql + Keycloak
SqlServer  + None
SqlServer  + Keycloak
```

## Validation rules

### Common rules

Every generated project should still satisfy the existing provider-clean checks:

```text
no provider-specific filename leftovers
expected neutral provider files exist
no EffectiveDbProvider in generated project files
no provider MSBuild conditions in generated project files
no wrong-provider package references
```

### `--auth None`

Generated output should not contain:

```text
Keycloak package references
Keycloak AppHost container files
Keycloak realm files
Keycloak resource names
Keycloak-specific appsettings sections
JWT bearer Keycloak authority/audience settings
AUTH_MODE=keycloak assumptions
```

### `--auth Keycloak`

Generated output should contain:

```text
Keycloak package references
Keycloak AppHost setup
Keycloak realm file
Keycloak appsettings sections
OpenAPI auth configuration where applicable
```

## Proposed work branches

```text
work/auth-switch-scope
work/auth-current-state-inventory
work/auth-template-option
work/auth-appsettings-materialization
work/auth-keycloak-file-materialization
work/auth-validation-matrix
work/auth-docs
```

## Pull request flow

Implementation work should use focused work branches targeting the story branch:

```text
work/* -> story/add-auth-template-switch
```

The completed story branch should target `main`:

```text
story/add-auth-template-switch -> main
```

## Open questions

1. Should `--auth None` remove all auth settings, or keep a minimal `Authentication.Enabled = false` section?
2. Should endpoint authorization metadata be conditionally removed for `None`, or should disabled auth make authorization no-op?
3. Should OpenAPI auth transformers be absent for `None`, or present but inactive?
4. Should smoke tests run against all four db/auth combinations, or should the full build/test matrix stay smaller?
5. Should Keycloak remain the default auth mode for backwards compatibility, or should `None` become the default for a lighter generated project?
