namespace Company.Template.CompositionRoot.Features;

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
