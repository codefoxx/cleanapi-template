using Company.Template.Composition.AspNetCore;
using Company.Template.Composition.Features;

namespace Company.Template.Api;

/// <summary>
///     Activates API pipeline behavior for shared HTTP cross-cutting concerns.
/// </summary>
/// <remarks>
///     Feature-specific endpoint mapping, security, authorization, and OpenAPI pipeline behavior are activated separately.
/// </remarks>
public sealed class ApiCrossCuttingWebAppModule : IFeatureWebAppModule<CrossCuttingConcerns>
{
    public void Use(FeatureWebAppContext context)
    {
        context.App.UseExceptionHandler();
    }
}
