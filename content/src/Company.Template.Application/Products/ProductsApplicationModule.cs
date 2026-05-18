using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Application.Products.DiscontinueProduct;
using Company.Template.Application.Products.GetProductById;
using Company.Template.Application.Products.GetProducts;
using Company.Template.Composition.Features;
using Company.Template.Composition.Framework;

namespace Company.Template.Application.Products;

/// <summary>
///     Registers application services owned by the Products feature.
/// </summary>
/// <remarks>
///     Use cases are registered explicitly so the feature module documents the application workflows it contributes.
/// </remarks>
public sealed class ProductsApplicationModule : IFeatureServiceModule<ProductsFeature>
{
    public void Register(FeatureServiceContext context)
    {
        context.Services.AddScoped<IUseCase<CreateProductCommand, ProductDto>, CreateProductUseCase>();
        context.Services.AddScoped<IUseCase<ChangeProductPriceCommand, ProductDto>, ChangeProductPriceUseCase>();
        context.Services.AddScoped<IUseCase<DiscontinueProductCommand>, DiscontinueProductUseCase>();
        context.Services.AddScoped<IUseCase<GetProductByIdQuery, ProductDto>, GetProductByIdUseCase>();
        context.Services.AddScoped<IUseCase<GetProductsQuery, PagedResult<ProductDto>>, GetProductsUseCase>();
    }
}
