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
content/src/Company.Template.Composition/appsettings.json
content/src/Company.Template.Composition/appsettings.Development.json
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

For `--auth None`, this section should probably disappear entirely unless the runtime code still requires `Authentication:Enabled=false`.

For `--auth Keycloak`, this section should stay and be materialized with the selected project names/placeholders.

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

`StartKeycloak` is a runtime switch for local orchestration.

For `--auth None`, the Keycloak-related AppHost settings should disappear.

For `--auth Keycloak`, they should stay because local Keycloak orchestration remains useful.

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

For `--auth None`, there are two possible designs:

1. remove these calls and possibly remove the auth extension/options files entirely
2. keep minimal no-op auth infrastructure and disable it by options

The story goal favors option 1 if feasible.

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

The pipeline uses runtime options to decide whether middleware is active.

For `--auth None`, this should ideally become a simple root endpoint mapping without resolving `AuthenticationOptions`.

For `--auth Keycloak`, current behavior can remain.

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

For `--auth None`, generated endpoints should probably not depend on `AuthenticationOptions` and should not call `RequireTemplatePolicy`.

For `--auth Keycloak`, the current conditional policy behavior is still useful because the appsettings default keeps auth disabled unless explicitly enabled.

Open question:

Should a Keycloak-materialized project still support runtime `Authentication:Enabled=false`, or should Keycloak mode mean authentication is structurally enabled?

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

Classification:

```text
Keycloak/JWT-specific enough to exclude from --auth None if possible.
```

However, `ICurrentUser` / `HttpCurrentUser` may remain useful as auth-neutral infrastructure if use cases depend on it.

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

For `--auth None`, this file should probably be excluded if no generated code references it.

For `--auth Keycloak`, this file stays.

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

For `--auth None`, OpenAPI should not register auth transformers and should not depend on `AuthenticationOptions`.

For `--auth Keycloak`, current behavior can remain.

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

For `--auth None`, generated AppHost should not contain:

```text
StartKeycloak
KeycloakUseDataVolume
Keycloak resource names
Keycloak package reference
Keycloak container extension files
Keycloak environment variable setup
```

For `--auth Keycloak`, current behavior can stay.

## Keycloak realm file

Relevant file:

```text
content/infra/keycloak/realms/__KEYCLOAK_REALM__-realm.json
```

For `--auth None`, the generated project should not contain `infra/keycloak`.

For `--auth Keycloak`, the realm file should remain and placeholders should be materialized.

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

For `--auth None`, these should be absent from generated `.props` / `.csproj` files.

For `--auth Keycloak`, they should remain.

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

This runtime smoke distinction is still useful, but after template materialization we also need generated-output checks for `--auth None` and `--auth Keycloak`.

Docs should distinguish:

```text
Template option: --auth None / --auth Keycloak
Runtime switch inside Keycloak-generated projects: Authentication:Enabled / AppHost:StartKeycloak
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

## Design questions to answer before code changes

### 1. Default auth mode

Options:

```text
Keycloak  // preserves current generated behavior most closely
None      // creates lighter default projects
```

Recommendation for now:

```text
Keycloak
```

Reason: lower migration risk while introducing the switch. We can change the default later if desired.

### 2. No-auth generated API shape

Should `--auth None` remove auth code entirely?

Recommendation:

```text
Yes, where practical.
```

Reason: generated projects should not look like Keycloak projects with runtime auth disabled.

### 3. Runtime disabling inside Keycloak mode

Should `--auth Keycloak` still allow `Authentication:Enabled=false`?

Recommendation:

```text
Yes, at least initially.
```

Reason: current local development and smoke-test behavior depends on authentication being disabled by default unless explicitly enabled.

### 4. OpenAPI files

Should OpenAPI auth transformers be removed for `None`?

Recommendation:

```text
Yes.
```

Reason: a no-auth OpenAPI document should not contain OAuth2/Bearer-related implementation paths.

### 5. `ICurrentUser`

Should `ICurrentUser` remain for `None`?

Recommendation:

```text
Probably yes.
```

Reason: use cases may depend on current-user abstraction even if the template starts with anonymous/system user behavior. This should be checked before removing any current-user infrastructure.

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
```

Generated `--auth Keycloak` projects should be checked for expected Keycloak assets:

```text
infra/keycloak/realms/<realm>-realm.json
Aspire.Hosting.Keycloak
Microsoft.AspNetCore.Authentication.JwtBearer
Authentication section
AppHost:StartKeycloak
```

## Non-goals

Do not use this story to redesign identity.

Do not add another identity provider.

Do not solve multi-tenancy.

Do not redesign feature composition.

Do not fix unrelated nullable composition diagnostics unless they directly block materialization validation.
