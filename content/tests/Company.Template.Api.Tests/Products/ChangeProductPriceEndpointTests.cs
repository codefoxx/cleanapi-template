using Company.Template.Api.Tests.Products.Contracts;

namespace Company.Template.Api.Tests.Products;

[Collection(DatabaseCollection.Name)]
public sealed class ChangeProductPriceEndpointTests
{
    private readonly TestDatabaseServer _server;

    public ChangeProductPriceEndpointTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task ChangeProductPrice_WithExistingProduct_ReturnsUpdatedProduct()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        ProductResponse createdProduct = await context.HttpClient.CreateProductAsync();

        ChangeProductPriceRequest request = ChangeProductPriceRequest.Valid()
                                                                     .WithPrice(249.50m)
                                                                     .WithCurrency("EUR");

        // Act
        HttpResponseMessage response = await context.HttpClient.SendJsonAsync(
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
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendJsonAsync(
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
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        ProductResponse createdProduct = await context.HttpClient.CreateProductAsync();

        ChangeProductPriceRequest request = ChangeProductPriceRequest.Valid()
                                                                     .WithCurrency("EU");

        // Act
        HttpResponseMessage response = await context.HttpClient.SendJsonAsync(
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
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        ProductResponse createdProduct = await context.HttpClient.CreateProductAsync();

        ChangeProductPriceRequest request = ChangeProductPriceRequest.Valid()
                                                                     .WithPrice(-1m);

        // Act
        HttpResponseMessage response = await context.HttpClient.SendJsonAsync(
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
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        ProductResponse discontinuedProduct = await context.HttpClient.CreateDiscontinuedProductAsync();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendJsonAsync(
            ProductEndpoints.ChangePrice(discontinuedProduct.Id),
            ChangeProductPriceRequest.Valid());

        // Assert
        await response.ShouldBeConflictProblemAsync("discontinued_product_cannot_be_changed");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.Problem);
    }
}
