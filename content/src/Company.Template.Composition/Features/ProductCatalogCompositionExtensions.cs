using Company.Template.Composition.Framework;

namespace Company.Template.Composition.Features;

/// <summary>
///     Defines the Products feature catalog used by the generated template.
/// </summary>
public static class ProductCatalogCompositionExtensions
{
    extension(FeatureCompositionContext context)
    {
        public FeatureCompositionContext AddProductCatalog()
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Add<ProductsFeature>();
        }
    }
}
