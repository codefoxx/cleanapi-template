using Company.Template.Application.Products;
using Company.Template.Composition.Abstractions.Features;
using Company.Template.Infrastructure.Persistence.Queries;

namespace Company.Template.Infrastructure.Products;

/// <summary>
///     Registers infrastructure adapters owned by the Products feature.
/// </summary>
/// <remarks>
///     Query adapters are registered from the feature module so the composition root activates only selected feature
///     infrastructure instead of scanning every query adapter in the assembly.
/// </remarks>
public sealed class ProductsInfrastructureModule : IFeatureServiceModule<ProductsFeature>
{
    public void Register(FeatureServiceContext context)
    {
        context.Services.AddScoped<IProductQueries, ProductQueries>();
    }
}
