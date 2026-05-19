using Company.Template.Composition.AspNetCore;
using Company.Template.Composition.Features;

namespace Company.Template.Api.OpenApi;

/// <summary>
///     Maps OpenAPI endpoints when API documentation should be exposed.
/// </summary>
public sealed class OpenApiWebAppModule : IFeatureWebAppModule<OpenApiFeature>
{
    public void Use(FeatureWebAppContext context)
    {
        if (context.App.Environment.IsDevelopment() || context.App.Environment.IsEnvironment("Testing"))
        {
            context.App.MapOpenApi();
        }
    }
}
