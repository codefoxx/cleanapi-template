namespace Company.Template.Api.Tests.Api;

public sealed class OpenApiMetadataTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public OpenApiMetadataTests(ApiTestFactory factory)
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

        document.ShouldAdvertiseResponse(HttpMethod.Get, "/api/products/{id}", 200);
        document.ShouldAdvertiseResponse(HttpMethod.Get, "/api/products/{id}", 404);

        document.ShouldAdvertiseResponse(HttpMethod.Put, "/api/products/{id}/price", 200);
        document.ShouldAdvertiseResponse(HttpMethod.Put, "/api/products/{id}/price", 400);
        document.ShouldAdvertiseResponse(HttpMethod.Put, "/api/products/{id}/price", 422);
        document.ShouldAdvertiseResponse(HttpMethod.Put, "/api/products/{id}/price", 404);
        document.ShouldAdvertiseResponse(HttpMethod.Put, "/api/products/{id}/price", 409);

        document.ShouldAdvertiseResponse(HttpMethod.Post, "/api/products/{id}/discontinue", 204);
        document.ShouldAdvertiseResponse(HttpMethod.Post, "/api/products/{id}/discontinue", 404);
        document.ShouldAdvertiseResponse(HttpMethod.Post, "/api/products/{id}/discontinue", 409);
    }
}
