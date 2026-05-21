# Auth Template Switch — Current-State Inventory

Branch:

```text
work/auth-inventory
```

Story branch:

```text
story/add-auth-template-switch
```

## Purpose

This document records how authentication is currently wired before adding the `--auth` template option.

The goal is to avoid changing code before we understand which parts are:

- auth-neutral
- Keycloak-specific
- runtime-toggle mechanics
- generated-output leftovers that should disappear for `--auth None`

## Auth mode decision

The story has a clear materialization decision:

```text
--auth None
  means no authentication artifacts in the materialized project.

--auth Keycloak
  means authentication is enforced, JWT bearer is configured for the sample Keycloak realm,
  and AppHost starts a Keycloak instance for the selected database provider.
```

This is stricter than the current runtime-toggle model.

A `None` project must not look like a Keycloak project with authentication disabled.

A `Keycloak` project must not require the developer to manually enable authentication after generation. It should be structurally and operationally configured for the Keycloak sample setup.

## Current runtime model

Authentication is currently a runtime setting, not a template materialization choice.

The generated application always contains authentication-related code and packages, but `appsettings` disables authentication by default:

```json
{
  "Authentication": {
    "Enabled": false,
    "Authority": "http://localhost:8080/realms/__KEYCLOAK_REALM__",
    "Audience": "__AUTH_AUDIENCE__",
    "RequireHttpsMetadata": false,
    "RoleClaimType": "roles"
  }
}
```

This means the current no-auth experience is actually:

```text
Auth code present
JWT bearer package present
OpenAPI auth transformers present
Keycloak placeholders present
Authentication disabled at runtime
```

For a template option, that is not clean enough.

## Runtime configuration files

Relevant files:

```text
content/src/Company.Template.CompositionRoot/appsettings.json
content/src/Company.Template.CompositionRoot/appsettings.Development.json
content/src/Company.Template.AppHost/appsettings.json
content/src/Company.Template.AppHost/appsettings.Development.json
content/src/Company.Template.MigrationService/appsettings.json
content/src/Company.Template.MigrationService/appsettings.Development.json
```

### Composition appsettings

`Company.Template.Composition/appsettings.json` contains the main runtime auth section.

Current behavior:

```text
Authentication:Enabled = false
Authentication:Authority = Keycloak realm placeholder
Authentication:Audience = auth audience placeholder
Authentication:RequireHttpsMetadata = false
Authentication:RoleClaimType = roles
```

Required materialized behavior:

```text
--auth None
  remove the Authentication section and all Keycloak/JWT settings.

--auth Keycloak
  keep the Authentication section, set Enabled=true, and configure Authority/Audience
  for the generated Keycloak sample realm.
```

### AppHost appsettings

`Company.Template.AppHost/appsettings.json` currently contains:

```json
{
  "AppHost": {
    "StartPgAdmin": false,
    "StartKeycloak": false,
    "KeycloakUseDataVolume": false
  }
}
```

`StartKeycloak` is currently a runtime switch for local orchestration.

Required materialized behavior:

```text
--auth None
  remove Keycloak-related AppHost settings.

--auth Keycloak
  keep Keycloak-related AppHost settings and default StartKeycloak=true.
```

Because Keycloak mode means auth is enforced, AppHost should start the Keycloak instance by default.

## API service registration

Relevant files:

```text
content/src/Company.Template.Api/ApiAdapterServiceModule.cs
content/src/Company.Template.Api/ApiAdapterWebAppModule.cs
content/src/Company.Template.Api/Security/AuthenticationExtensions.cs
content/src/Company.Template.Api/Options/AuthenticationOptions.cs
content/src/Company.Template.Api/GlobalUsings.cs
```

### ApiAdapterServiceModule

Current behavior:

```csharp
context.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

context.Services.AddTemplateAuthentication();
context.Services.AddTemplateAuthorization();
```

Authentication and authorization services are always registered.

Required materialized behavior:

```text
--auth None
  remove authentication/authorization registration and exclude Keycloak/JWT-specific files.

--auth Keycloak
  keep authentication/authorization registration and enforce the configured JWT setup.
```

`ICurrentUser` may remain auth-neutral if the application still needs a current-user abstraction, but it must not drag in JWT/Keycloak artifacts for `--auth None`.

### ApiAdapterWebAppModule

Current behavior:

```csharp
AuthenticationOptions authenticationOptions = context.App.Services
                                                     .GetRequiredService<IOptions<AuthenticationOptions>>()
                                                     .Value;

if (authenticationOptions.Enabled)
{
    context.App.UseAuthentication();
    context.App.UseAuthorization();
}
```

The pipeline currently uses runtime options to decide whether middleware is active.

Required materialized behavior:

```text
--auth None
  no auth middleware and no AuthenticationOptions dependency.

--auth Keycloak
  always use authentication and authorization middleware.
```

## Endpoint authorization metadata

Relevant file:

```text
content/src/Company.Template.Api/Endpoints/Products/ProductEndpoints.cs
```

Current behavior:

```text
Product endpoints resolve AuthenticationOptions from DI.
RequireTemplatePolicy(policy, authenticationOptions.Enabled) conditionally calls RequireAuthorization.
```

This gives runtime toggling.

Required materialized behavior:

```text
--auth None
  endpoints should not depend on AuthenticationOptions and should not call RequireAuthorization.

--auth Keycloak
  endpoints should require authorization according to the sample policies.
```

## API authentication implementation

Relevant file:

```text
content/src/Company.Template.Api/Security/AuthenticationExtensions.cs
```

Current responsibilities:

- `RequireTemplatePolicy(...)` conditionally applies endpoint authorization.
- `AddTemplateAuthentication()` binds `AuthenticationOptions`.
- Validates authority/audience/role claim type only when enabled.
- Registers JWT bearer authentication.
- Configures `JwtBearerOptions` from `AuthenticationOptions`.
- `AddTemplateAuthorization()` adds product read/write policies.

Required materialized behavior:

```text
--auth None
  exclude this file or replace it with an auth-free equivalent if needed.

--auth Keycloak
  keep JWT bearer authentication, remove runtime-disabled behavior where practical,
  and validate Keycloak configuration as required startup configuration.
```

## Authentication options

Relevant file:

```text
content/src/Company.Template.Api/Options/AuthenticationOptions.cs
```

Current shape:

```csharp
internal sealed class AuthenticationOptions
{
    public const string DefaultAudience = "company-template-api";
    public const string DefaultRoleClaimType = "roles";
    public const string SectionName = "Authentication";
    public string Audience { get; init; } = DefaultAudience;
    public string Authority { get; init; } = "";
    public bool Enabled { get; init; }
    public bool RequireHttpsMetadata { get; init; }
    public string RoleClaimType { get; init; } = DefaultRoleClaimType;
}
```

Required materialized behavior:

```text
--auth None
  exclude this file if no generated code references it.

--auth Keycloak
  keep this file, but consider whether Enabled is still needed once Keycloak mode is enforced.
```

## OpenAPI auth metadata

Relevant files:

```text
content/src/Company.Template.Api/OpenApi/AuthenticationOpenApiTransformers.cs
content/src/Company.Template.Api/OpenApi/AuthorizationOperationTransformer.cs
content/src/Company.Template.Api/OpenApi/BearerSecuritySchemeTransformer.cs
content/src/Company.Template.Api/OpenApi/OAuth2SecuritySchemeTransformer.cs
content/src/Company.Template.Api/OpenApi/OpenApiExtensions.cs
```

Current behavior:

- `OpenApiExtensions.AddTemplateOpenApi()` always registers auth-aware document/operation transformers.
- The transformers check `AuthenticationOptions.Enabled` before adding auth metadata.
- OAuth2 metadata points to the configured Keycloak authority.

Required materialized behavior:

```text
--auth None
  no auth OpenAPI transformers and no AuthenticationOptions dependency.

--auth Keycloak
  OpenAPI should include Bearer/OAuth2 metadata for the Keycloak sample realm.
```

Potential materialization strategy:

```text
OpenApiExtensions.None.cs       -> OpenApiExtensions.cs
OpenApiExtensions.Keycloak.cs   -> OpenApiExtensions.cs
```

or keep one file with template-time conditional registration.

Prefer the approach that produces the cleanest generated output with minimal conditional noise.

## AppHost Keycloak wiring

Relevant files:

```text
content/src/Company.Template.AppHost/AppHostNames.cs
content/src/Company.Template.AppHost/Program.cs
content/src/Company.Template.AppHost/Company.Template.AppHost.csproj
content/src/Company.Template.AppHost/Containers/KeycloakContainerExtensions.cs
content/src/Company.Template.AppHost/Containers/KeycloakContainerOptions.cs
```

Current behavior:

- `AppHostNames` always defines Keycloak realm/resource names.
- `Program.cs` always reads `AppHost:StartKeycloak`.
- If enabled, AppHost creates Keycloak and passes authentication environment variables to the API.
- AppHost project always references `Aspire.Hosting.Keycloak`.

Required materialized behavior:

```text
--auth None
  generated AppHost should not contain Keycloak resource names, Keycloak package references,
  Keycloak container files, Keycloak environment variables, or StartKeycloak settings.

--auth Keycloak
  generated AppHost should start Keycloak by default and wire API auth environment variables
  to the generated Keycloak realm/resource names.
```

Keycloak should use the selected database provider where possible.

That means the AppHost behavior for Keycloak must be checked against both generated database modes:

```text
PostgreSql + Keycloak
SqlServer  + Keycloak
```

## Keycloak realm file

Relevant file:

```text
content/infra/keycloak/realms/__KEYCLOAK_REALM__-realm.json
```

Required materialized behavior:

```text
--auth None
  generated project should not contain infra/keycloak.

--auth Keycloak
  generated project should contain the realm file with placeholders replaced for the generated sample realm.
```

## Package references

Relevant files:

```text
content/Directory.Packages.props
content/src/Company.Template.Api/Company.Template.Api.csproj
content/src/Company.Template.AppHost/Company.Template.AppHost.csproj
```

Current package references include:

```text
Aspire.Hosting.Keycloak
Microsoft.AspNetCore.Authentication.JwtBearer
```

Required materialized behavior:

```text
--auth None
  these package references should be absent from generated .props / .csproj files.

--auth Keycloak
  these package references should be present.
```

## Smoke scripts and docs

Relevant files:

```text
scripts/smoke/api-smoke.js
content/AUTHENTICATION.md
content/OPENAPI.md
content/TESTING.md
content/ASPIRE.md
README.md
content/README.md
```

Current smoke script supports:

```text
AUTH_MODE=none
AUTH_MODE=keycloak
```

After this story, docs and smoke behavior should distinguish:

```text
Template option: --auth None / --auth Keycloak

--auth None:
  no authentication artifacts exist in the generated project.

--auth Keycloak:
  authentication is enforced and AppHost starts Keycloak by default.
```

## Suggested implementation strategy

Do not try to solve everything in one branch.

Recommended order:

1. Add `auth` template choice without changing generated output yet.
2. Materialize package references for `None` vs `Keycloak`.
3. Materialize AppHost Keycloak files and appsettings.
4. Materialize API auth/security/OpenAPI files or sections.
5. Extend validation matrix.
6. Update docs.

## Design decisions

### 1. Default auth mode

Options:

```text
Keycloak  // preserves the richer sample setup
None      // creates lighter default projects
```

Decision still open.

Recommendation to decide before implementation:

```text
Prefer None for a minimal default template, or Keycloak if preserving the current full sample is more important.
```

### 2. No-auth generated API shape

Decision:

```text
--auth None removes auth artifacts from the materialized project.
```

This includes Keycloak, JWT bearer, OpenAPI auth metadata, and auth-specific appsettings.

### 3. Runtime disabling inside Keycloak mode

Decision:

```text
--auth Keycloak enforces authentication.
```

Do not keep `Authentication:Enabled=false` as the default in Keycloak-generated projects.

If `Enabled` remains in the options model temporarily, the generated Keycloak configuration must set it to `true`.

### 4. OpenAPI files

Decision:

```text
OpenAPI auth transformers are removed for --auth None and present for --auth Keycloak.
```

A no-auth OpenAPI document should not contain OAuth2/Bearer implementation paths.

### 5. `ICurrentUser`

Open question:

```text
Should ICurrentUser remain for --auth None?
```

Recommendation:

```text
Probably yes, if a neutral anonymous/system current-user implementation is useful.
```

But it must not drag in JWT/Keycloak artifacts for `--auth None`.

## Validation additions needed later

Generated `--auth None` projects should fail validation if they contain:

```text
Keycloak
Authentication:Authority
Authentication:Audience
Aspire.Hosting.Keycloak
Microsoft.AspNetCore.Authentication.JwtBearer
BearerSecuritySchemeTransformer
OAuth2SecuritySchemeTransformer
__KEYCLOAK_REALM__
__KEYCLOAK_RESOURCE_NAME__
StartKeycloak
KeycloakUseDataVolume
```

Generated `--auth Keycloak` projects should be checked for expected Keycloak assets:

```text
infra/keycloak/realms/<realm>-realm.json
Aspire.Hosting.Keycloak
Microsoft.AspNetCore.Authentication.JwtBearer
Authentication section with Enabled=true
AppHost:StartKeycloak=true
Keycloak AppHost container wiring
OpenAPI auth metadata
endpoint authorization metadata
```

## Non-goals

Do not use this story to redesign identity.

Do not add another identity provider.

Do not solve multi-tenancy.

Do not redesign feature composition.

Do not fix unrelated nullable composition diagnostics unless they directly block materialization validation.
