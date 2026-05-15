namespace Company.Template.Api.Tests.Products;

internal static class ProductEndpoints
{
    public static readonly ApiEndpoint Collection = ApiEndpoint.Get("/api/products");
    public static readonly ApiEndpoint Create = ApiEndpoint.Post("/api/products");

    public static ApiEndpoint CollectionWithQuery(string queryString)
    {
        return ApiEndpoint.Get($"/api/products?{queryString.TrimStart('?')}");
    }

    public static ApiEndpoint ById(Guid productId)
    {
        return ApiEndpoint.Get($"/api/products/{productId}");
    }

    public static ApiEndpoint ChangePrice(Guid productId)
    {
        return ApiEndpoint.Put($"/api/products/{productId}/price");
    }

    public static ApiEndpoint Discontinue(Guid productId)
    {
        return ApiEndpoint.Post($"/api/products/{productId}/discontinue");
    }
}
