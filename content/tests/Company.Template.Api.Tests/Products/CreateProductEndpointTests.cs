using Company.Template.Api.Tests.Products.Contracts;

namespace Company.Template.Api.Tests.Products;

public sealed class CreateProductEndpointTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public CreateProductEndpointTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateProduct_WithValidRequest_ReturnsCreatedProduct()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        CreateProductRequest request = CreateProductRequest.Valid();

        // Act
        HttpResponseMessage response = await httpClient.SendJsonAsync(
            ProductEndpoints.Create,
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        ProductResponse product = await response.ReadJsonAsync<ProductResponse>();

        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.ToString().ShouldBe($"/api/products/{product.Id}");

        product.Id.ShouldNotBe(Guid.Empty);
        product.Name.ShouldBe(request.Name);
        product.Price.Amount.ShouldBe(request.Price);
        product.Price.Currency.ShouldBe(request.Currency);
        product.Status.ShouldBe("Active");
        product.CreatedAt.ShouldNotBe(default);
        product.DiscontinuedAt.ShouldBeNull();

        await response.Content.ShouldContainJsonPathsAsync(ProductJsonContracts.Product);
    }

    [Fact]
    public async Task CreateProduct_WithEmptyName_ReturnsValidationProblem()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        CreateProductRequest request = CreateProductRequest.Valid()
                                                            .WithName("");

        // Act
        HttpResponseMessage response = await httpClient.SendJsonAsync(
            ProductEndpoints.Create,
            request);

        // Assert
        ApiProblemDetails problem = await response.ShouldBeValidationProblemAsync("product_name_required");

        problem.Errors!["request"].ShouldContain("Product name is required.");

        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.ValidationProblem);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidCurrency_ReturnsValidationProblem()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        CreateProductRequest request = CreateProductRequest.Valid()
                                                            .WithCurrency("US");

        // Act
        HttpResponseMessage response = await httpClient.SendJsonAsync(
            ProductEndpoints.Create,
            request);

        // Assert
        await response.ShouldBeValidationProblemAsync("currency_invalid_format");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.ValidationProblem);
    }

    [Fact]
    public async Task CreateProduct_WithNegativePrice_ReturnsValidationProblem()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();
        CreateProductRequest request = CreateProductRequest.Valid()
                                                            .WithPrice(-1m);

        // Act
        HttpResponseMessage response = await httpClient.SendJsonAsync(
            ProductEndpoints.Create,
            request);

        // Assert
        await response.ShouldBeValidationProblemAsync("amount_negative");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.ValidationProblem);
    }
}
