using Company.Template.Api.Tests.Products.Contracts;

namespace Company.Template.Api.Tests.Products;

[Collection(DatabaseCollection.Name)]
public sealed class GetProductByIdEndpointTests
{
    private readonly TestDatabaseServer _server;

    public GetProductByIdEndpointTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task GetProductById_WithExistingProduct_ReturnsProduct()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        ProductResponse createdProduct = await context.HttpClient.CreateProductAsync(
            CreateProductRequest.Valid()
                                .WithName("Mechanical Keyboard"));

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.ById(createdProduct.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        ProductResponse product = await response.ReadJsonAsync<ProductResponse>();

        product.Id.ShouldBe(createdProduct.Id);
        product.Name.ShouldBe("Mechanical Keyboard");
        product.Status.ShouldBe("Active");
        product.DiscontinuedAt.ShouldBeNull();

        await response.Content.ShouldContainJsonPathsAsync(ProductJsonContracts.Product);
    }

    [Fact]
    public async Task GetProductById_WithUnknownProduct_ReturnsNotFoundProblem()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.ById(Guid.NewGuid()));

        // Assert
        await response.ShouldBeNotFoundProblemAsync("not_found");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.Problem);
    }

    [Fact]
    public async Task GetProductById_WithDiscontinuedProduct_ReturnsProductSnapshot()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();
        ProductResponse discontinuedProduct = await context.HttpClient.CreateDiscontinuedProductAsync();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.ById(discontinuedProduct.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        ProductResponse product = await response.ReadJsonAsync<ProductResponse>();

        product.Id.ShouldBe(discontinuedProduct.Id);
        product.Status.ShouldBe("Discontinued");
        product.DiscontinuedAt.ShouldNotBeNull();

        await response.Content.ShouldContainJsonPathsAsync(ProductJsonContracts.Product);
    }
}
