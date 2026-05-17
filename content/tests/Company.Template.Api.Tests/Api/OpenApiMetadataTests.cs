using Company.Template.Api.Routing;

namespace Company.Template.Api.Tests.Api;

public sealed class OpenApiMetadataTests : IClassFixture<ApiLightweightTestFactory>
{
    private readonly ApiLightweightTestFactory _factory;

    public OpenApiMetadataTests(ApiLightweightTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OpenApiDocument_ProductEndpoints_ShouldAdvertiseExpectedResponses()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();

        // Act
        using JsonDocument document = await httpClient.GetOpenApiDocumentAsync();

        // Assert
        document.ShouldAdvertiseResponse(HttpMethod.Get, ProductPaths.Collection, 200);
        document.ShouldAdvertiseResponse(HttpMethod.Get, ProductPaths.Collection, 400);
        document.ShouldAdvertiseResponse(HttpMethod.Get, ProductPaths.Collection, 422);

        document.ShouldAdvertiseResponse(HttpMethod.Post, ProductPaths.Collection, 201);
        document.ShouldAdvertiseResponse(HttpMethod.Post, ProductPaths.Collection, 400);
        document.ShouldAdvertiseResponse(HttpMethod.Post, ProductPaths.Collection, 422);

        document.ShouldAdvertiseResponse(HttpMethod.Get, ProductPaths.ById, 200);
        document.ShouldAdvertiseResponse(HttpMethod.Get, ProductPaths.ById, 404);

        document.ShouldAdvertiseResponse(HttpMethod.Put, ProductPaths.Price, 200);
        document.ShouldAdvertiseResponse(HttpMethod.Put, ProductPaths.Price, 400);
        document.ShouldAdvertiseResponse(HttpMethod.Put, ProductPaths.Price, 422);
        document.ShouldAdvertiseResponse(HttpMethod.Put, ProductPaths.Price, 404);
        document.ShouldAdvertiseResponse(HttpMethod.Put, ProductPaths.Price, 409);

        document.ShouldAdvertiseResponse(HttpMethod.Post, ProductPaths.Discontinue, 204);
        document.ShouldAdvertiseResponse(HttpMethod.Post, ProductPaths.Discontinue, 404);
        document.ShouldAdvertiseResponse(HttpMethod.Post, ProductPaths.Discontinue, 409);
    }

    private static class ProductPaths
    {
        public const string Collection = ApiRoutes.Products.Group;
        public const string ById = $"{ApiRoutes.Products.Group}{ApiRoutes.Products.ById}";
        public const string Price = $"{ApiRoutes.Products.Group}{ApiRoutes.Products.Price}";
        public const string Discontinue = $"{ApiRoutes.Products.Group}{ApiRoutes.Products.Discontinue}";
    }
}
