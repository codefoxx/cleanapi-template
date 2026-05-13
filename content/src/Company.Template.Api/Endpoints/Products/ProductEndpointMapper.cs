using Company.Template.Application.Products;

namespace Company.Template.Api.Endpoints.Products;

internal static class ProductEndpointMapper
{
    public static ProductResponse ToResponse(ProductDto product)
    {
        return new ProductResponse(
            product.Id,
            product.Name,
            new MoneyResponse(product.Price.Amount, product.Price.Currency.Code),
            product.Status.ToString(),
            product.CreatedAt,
            product.DiscontinuedAt);
    }
}
