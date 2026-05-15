using Company.Template.Api.Tests.Products.Contracts;

namespace Company.Template.Api.Tests.Products;

public sealed class DiscontinueProductEndpointTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public DiscontinueProductEndpointTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DiscontinueProduct_WithExistingProduct_ReturnsNoContent()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        ProductResponse product = await httpClient.CreateProductAsync();

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
            ProductEndpoints.Discontinue(product.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DiscontinueProduct_WithUnknownProduct_ReturnsNotFoundProblem()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
            ProductEndpoints.Discontinue(Guid.NewGuid()));

        // Assert
        await response.ShouldBeNotFoundProblemAsync("not_found");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.Problem);
    }

    [Fact]
    public async Task DiscontinueProduct_WhenAlreadyDiscontinued_ReturnsNoContent()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        ProductResponse product = await httpClient.CreateDiscontinuedProductAsync();

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
            ProductEndpoints.Discontinue(product.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
