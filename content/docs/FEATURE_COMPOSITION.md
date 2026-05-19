# FEATURE COMPOSITION

> How the generated solution activates features without hiding the architecture.

## Why feature composition exists

The generated API needs to wire together API adapters, application use cases, infrastructure adapters, persistence,
OpenAPI, observability, and other technical concerns. A flat startup file with many unrelated `AddXyz()` calls becomes
hard to scan as the sample grows.

Feature composition gives the composition root a small vocabulary:

```csharp
builder.Services
       .AddFeatureServicesFromAssemblies(
            typeof(ApiAssemblyMarker).Assembly,
            typeof(ApplicationAssemblyMarker).Assembly,
            typeof(InfrastructureAssemblyMarker).Assembly)
       .WithConfiguration(builder.Configuration)
       .ComposeFeatures(features => features
           .AddTemplateDefaults()
           .AddProductCatalog()
           .DecorateUseCasesWithTelemetry());
```

The goal is not to build a generic plugin framework. The goal is readable startup code where selected features are
visible, layer boundaries remain explicit, and each layer keeps owning its registrations.

## Feature markers

A feature marker is an empty type that implements `IFeature`, for example `ProductsFeature`.

Markers are intentionally empty type tokens. They should not contain configuration, service registration, or behavior.
They exist so service and web modules in different assemblies can agree on one feature identity without relying on
string names or folder conventions.

Markers live in the feature catalog:

```text
src/Company.Template.Composition.Abstractions/FeatureCatalog/
```

This keeps feature identities in one shared place while avoiding dependencies from Application or Infrastructure to
the executable composition root.

## Service modules

Service modules implement `IFeatureServiceModule<TFeature>`.

A single feature can have service modules in multiple layers:

- API can register HTTP-adapter services.
- Application can register use cases.
- Infrastructure can register persistence adapters and other outbound adapters.

`AddFeatureServicesFromAssemblies(...)` tells the composition mechanism which assemblies may contain modules.
`.ComposeFeatures(...)` then activates only the selected features. When `AddProductCatalog()` activates
`ProductsFeature`, the builder discovers and runs every `IFeatureServiceModule<ProductsFeature>` from those assemblies.

## Composition helpers

The generated `Company.Template.Composition` project contains small helper methods that describe the template's
startup policy:

```csharp
.ComposeFeatures(features => features
    .AddTemplateDefaults()
    .AddProductCatalog()
    .DecorateUseCasesWithTelemetry());
```

`AddTemplateDefaults()` activates technical defaults such as the API adapter boundary, persistence, OpenAPI service
registrations, domain events, and generic cross-cutting services.

`AddProductCatalog()` activates the sample Products feature.

`DecorateUseCasesWithTelemetry()` queues the use-case telemetry decorator feature.

These helpers are meant to keep `Program.cs` readable. They should stay small and direct. If a helper starts hiding
too much behavior, split it or move the detail back into an obvious feature module.

## Decorators

Decorators are queued during `.ComposeFeatures(...)` and applied after normal feature service registration.

That order matters. Scrutor decoration wraps services that already exist in the service collection. If a decorator ran
inside normal `.Add<TFeature>()` registration, it could execute before all use cases had been registered, leaving some
use cases undecorated.

Decorator modules implement:

```csharp
IFeatureServiceDecoratorModule<TDecoratedFeature, TDecoratorFeature>
```

For use-case telemetry, `ApplicationUseCasesFeature` represents the decorated service group and
`UseCaseTelemetryFeature` represents the decorator feature. The marker types remain empty; the decorator module owns
the Scrutor calls.

## ASP.NET Core pipeline composition

Service registration and `WebApplication` pipeline composition are separate.

Service composition uses `Company.Template.Composition.Abstractions` and runs through `IServiceCollection`.
ASP.NET Core pipeline composition uses `Company.Template.Composition.AspNetCore` and runs through `WebApplication`:

```csharp
app.UseFeaturesFromAssemblies(typeof(ApiAssemblyMarker).Assembly)
   .Use<CrossCuttingConcerns>()
   .Use<OpenApiFeature>()
   .Use<ApiAdapterFeature>()
   .Use<ProductsFeature>();
```

`.Use<TFeature>()` discovers `IFeatureWebAppModule<TFeature>` modules. Web modules can map endpoints, apply middleware,
or add other HTTP adapter pipeline configuration for a selected feature.

Keeping this separate prevents ASP.NET Core abstractions from leaking into Application or Infrastructure. Those layers
can reference service-composition abstractions without taking a dependency on `Microsoft.AspNetCore.App`.

## Folder and namespace roles

`Company.Template.Composition.Abstractions` contains service-composition pieces:

- `Contracts`: feature contracts, service module contracts, decorator module contracts, and service composition
  extension methods.
- `Contexts`: context objects passed into service modules and composition helpers.
- `FeatureCatalog`: empty feature markers shared across layers.

`Company.Template.Composition.AspNetCore` contains ASP.NET Core pipeline composition pieces:

- `Contracts`: web app module contracts and `UseFeaturesFromAssemblies(...)`.
- `Contexts`: context objects passed into web app modules.

## What to edit

Usually edit:

- `Company.Template.Composition`
- feature modules in `Api`, `Application`, and `Infrastructure`

Usually do not edit unless extending the composition mechanism:

- `Company.Template.Composition.Abstractions`
- `Company.Template.Composition.AspNetCore`

That split is deliberate. Most application changes should add or adjust a feature module, not modify the composition
mechanism itself.

---

## Related documents

- [README](../README.md)
- [ARCHITECTURE](../ARCHITECTURE.md)
- [APPLICATION](../APPLICATION.md)
- [API](../API.md)
- [FEATURES](../FEATURES.md)
- [TESTING](../TESTING.md)
