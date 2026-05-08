using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

internal static class ProductMapper
{
    public static ProductDto ToDto(Product product)
    {
        return new ProductDto(
            product.Id.Value,
            product.Name.Value,
            product.Price.Amount,
            product.Price.Currency,
            product.Status,
            product.CreatedAt,
            product.DiscontinuedAt);
    }
}
