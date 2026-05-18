using Company.Template.Composition.Abstractions.Features;

namespace Company.Template.Api.OpenApi;

/// <summary>
///     Registers OpenAPI services for generated API documentation.
/// </summary>
public sealed class OpenApiServiceModule : IFeatureServiceModule<OpenApiFeature>
{
    public void Register(FeatureServiceContext context)
    {
        context.Services.AddTemplateOpenApi();
    }
}
