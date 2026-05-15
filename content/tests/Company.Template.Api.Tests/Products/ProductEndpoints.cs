namespace Company.Template.Api.Tests.Products;

internal static class ProductEndpoints
{
    public static readonly ApiEndpoint Collection = ApiEndpoint.Get("/api/products");
    public static readonly ApiEndpoint Create = ApiEndpoint.Post("/api/products");

    public static ApiEndpoint ById(Guid id)
    {
        return ApiEndpoint.Get($"/api/products/{id}");
    }

    public static ApiEndpoint ChangePrice(Guid id)
    {
        return ApiEndpoint.Put($"/api/products/{id}/price");
    }

    public static ApiEndpoint Discontinue(Guid id)
    {
        return ApiEndpoint.Post($"/api/products/{id}/discontinue");
    }
}
