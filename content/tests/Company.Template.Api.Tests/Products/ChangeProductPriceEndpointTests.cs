using Company.Template.Api.Tests.Products.Contracts;

namespace Company.Template.Api.Tests.Products;

public sealed class ChangeProductPriceEndpointTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public ChangeProductPriceEndpointTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ChangeProductPrice_WithExistingProduct_ReturnsUpdatedProduct()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        ProductResponse createdProduct = await httpClient.CreateProductAsync();

        ChangeProductPriceRequest request = ChangeProductPriceRequest.Valid()
                                                                     .WithPrice(249.50m)
                                                                     .WithCurrency("EUR");

        // Act
        HttpResponseMessage response = await httpClient.SendJsonAsync(
            ProductEndpoints.ChangePrice(createdProduct.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        ProductResponse product = await response.ReadJsonAsync<ProductResponse>();

        product.Id.ShouldBe(createdProduct.Id);
        product.Price.Amount.ShouldBe(249.50m);
        product.Price.Currency.ShouldBe("EUR");
        product.Status.ShouldBe("Active");

        await response.Content.ShouldContainJsonPathsAsync(ProductJsonContracts.Product);
    }

    [Fact]
    public async Task ChangeProductPrice_WithUnknownProduct_ReturnsNotFoundProblem()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.SendJsonAsync(
            ProductEndpoints.ChangePrice(Guid.NewGuid()),
            ChangeProductPriceRequest.Valid());

        // Assert
        await response.ShouldBeNotFoundProblemAsync("not_found");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.Problem);
    }

    [Fact]
    public async Task ChangeProductPrice_WithInvalidCurrency_ReturnsValidationProblem()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        ProductResponse createdProduct = await httpClient.CreateProductAsync();

        ChangeProductPriceRequest request = ChangeProductPriceRequest.Valid()
                                                                     .WithCurrency("EU");

        // Act
        HttpResponseMessage response = await httpClient.SendJsonAsync(
            ProductEndpoints.ChangePrice(createdProduct.Id),
            request);

        // Assert
        await response.ShouldBeValidationProblemAsync("currency_invalid_format");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.ValidationProblem);
    }

    [Fact]
    public async Task ChangeProductPrice_WithNegativePrice_ReturnsValidationProblem()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        ProductResponse createdProduct = await httpClient.CreateProductAsync();

        ChangeProductPriceRequest request = ChangeProductPriceRequest.Valid()
                                                                     .WithPrice(-1m);

        // Act
        HttpResponseMessage response = await httpClient.SendJsonAsync(
            ProductEndpoints.ChangePrice(createdProduct.Id),
            request);

        // Assert
        await response.ShouldBeValidationProblemAsync("amount_negative");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.ValidationProblem);
    }

    [Fact]
    public async Task ChangeProductPrice_WithDiscontinuedProduct_ReturnsConflictProblem()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        ProductResponse discontinuedProduct = await httpClient.CreateDiscontinuedProductAsync();

        // Act
        HttpResponseMessage response = await httpClient.SendJsonAsync(
            ProductEndpoints.ChangePrice(discontinuedProduct.Id),
            ChangeProductPriceRequest.Valid());

        // Assert
        await response.ShouldBeConflictProblemAsync("discontinued_product_cannot_be_changed");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.Problem);
    }
}
