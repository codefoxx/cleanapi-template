using System.Linq.Expressions;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

internal static class ProductMapper
{
    public static readonly Expression<Func<Product, ProductDto>> ToDtoExpression =
        product => new ProductDto(
            product.Id.Value,
            product.Name.Value,
            product.Price,
            product.Status,
            product.CreatedAt,
            product.DiscontinuedAt);

    public static ProductDto ToDto(Product product)
    {
        return new ProductDto(
            product.Id.Value,
            product.Name.Value,
            product.Price,
            product.Status,
            product.CreatedAt,
            product.DiscontinuedAt);
    }
}
