using Company.Template.Api.Tests.Products.Contracts;

namespace Company.Template.Api.Tests.Products;

public sealed class GetProductByIdEndpointTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public GetProductByIdEndpointTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProductById_WithExistingProduct_ReturnsProduct()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();

        ProductResponse createdProduct = await httpClient.CreateProductAsync(
            CreateProductRequest.Valid()
                                .WithName("Mechanical Keyboard"));

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
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
        using HttpClient httpClient = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
            ProductEndpoints.ById(Guid.NewGuid()));

        // Assert
        await response.ShouldBeNotFoundProblemAsync("not_found");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.Problem);
    }

    [Fact]
    public async Task GetProductById_WithDiscontinuedProduct_ReturnsProductSnapshot()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        ProductResponse discontinuedProduct = await httpClient.CreateDiscontinuedProductAsync();

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
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
