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
        document.ShouldAdvertiseResponse(HttpMethod.Get, "/api/products", 200);
        document.ShouldAdvertiseResponse(HttpMethod.Get, "/api/products", 400);
        document.ShouldAdvertiseResponse(HttpMethod.Get, "/api/products", 422);

        document.ShouldAdvertiseResponse(HttpMethod.Post, "/api/products", 201);
        document.ShouldAdvertiseResponse(HttpMethod.Post, "/api/products", 400);
        document.ShouldAdvertiseResponse(HttpMethod.Post, "/api/products", 422);

        document.ShouldAdvertiseResponse(HttpMethod.Get, "/api/products/{productId}", 200);
        document.ShouldAdvertiseResponse(HttpMethod.Get, "/api/products/{productId}", 404);

        document.ShouldAdvertiseResponse(HttpMethod.Put, "/api/products/{productId}/price", 200);
        document.ShouldAdvertiseResponse(HttpMethod.Put, "/api/products/{productId}/price", 400);
        document.ShouldAdvertiseResponse(HttpMethod.Put, "/api/products/{productId}/price", 422);
        document.ShouldAdvertiseResponse(HttpMethod.Put, "/api/products/{productId}/price", 404);
        document.ShouldAdvertiseResponse(HttpMethod.Put, "/api/products/{productId}/price", 409);

        document.ShouldAdvertiseResponse(HttpMethod.Post, "/api/products/{productId}/discontinue", 204);
        document.ShouldAdvertiseResponse(HttpMethod.Post, "/api/products/{productId}/discontinue", 404);
        document.ShouldAdvertiseResponse(HttpMethod.Post, "/api/products/{productId}/discontinue", 409);
    }
}
