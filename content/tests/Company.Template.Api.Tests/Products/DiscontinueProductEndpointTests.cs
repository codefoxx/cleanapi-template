using Company.Template.Api.Tests.Products.Contracts;

namespace Company.Template.Api.Tests.Products;

[Collection(DatabaseCollection.Name)]
public sealed class DiscontinueProductEndpointTests
{
    private readonly TestDatabaseServer _server;

    public DiscontinueProductEndpointTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task DiscontinueProduct_WithExistingProduct_ReturnsNoContent()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        ProductResponse product = await context.HttpClient.CreateProductAsync();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.Discontinue(product.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DiscontinueProduct_WithUnknownProduct_ReturnsNotFoundProblem()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.Discontinue(Guid.NewGuid()));

        // Assert
        await response.ShouldBeNotFoundProblemAsync("not_found");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.Problem);
    }

    [Fact]
    public async Task DiscontinueProduct_WhenAlreadyDiscontinued_ReturnsNoContent()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        ProductResponse product = await context.HttpClient.CreateDiscontinuedProductAsync();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.Discontinue(product.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
