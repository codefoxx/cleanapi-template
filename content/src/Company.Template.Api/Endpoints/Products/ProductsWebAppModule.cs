using Company.Template.Application.Products;
using Company.Template.Composition.Abstractions.Features;

namespace Company.Template.Api.Endpoints.Products;

/// <summary>
///     Activates the HTTP adapter pipeline owned by the Products feature.
/// </summary>
/// <remarks>
///     The current module maps product endpoints. The WebApplication-level contract leaves room for feature-owned
///     middleware or other HTTP adapter pipeline configuration later.
/// </remarks>
public sealed class ProductsWebAppModule : IFeatureWebAppModule<ProductsFeature>
{
    public void Use(FeatureWebAppContext context)
    {
        ProductEndpoints.MapEndpoints(context.App);
    }
}
