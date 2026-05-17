using Company.Template.Api.Tests.Products.Contracts;
using Company.Template.Domain.Common;

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
        ApiProblemDetails problem = await response.ReadNotFoundProblemAsync();

        problem.Title.ShouldBe("Resource not found.");
        problem.Status.ShouldBe((int)HttpStatusCode.NotFound);
        problem.Code.ShouldBe(DomainErrorCodes.NotFound.Value);
        problem.Detail.ShouldBe("Product was not found.");
        problem.Errors.ShouldBeNull();
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
        ApiProblemDetails problem = await response.ReadValidationProblemAsync();

        problem.Title.ShouldBe("Validation failed.");
        problem.Status.ShouldBe((int)HttpStatusCode.UnprocessableEntity);
        problem.Code.ShouldBe(DomainErrorCodes.CurrencyInvalidFormat.Value);
        problem.Detail.ShouldBe("Currency must be a three-letter ISO 4217 alphabetic code.");

        problem.Errors.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("request");
        problem.Errors["request"].ShouldContain("Currency must be a three-letter ISO 4217 alphabetic code.");
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
        ApiProblemDetails problem = await response.ReadValidationProblemAsync();

        problem.Title.ShouldBe("Validation failed.");
        problem.Status.ShouldBe((int)HttpStatusCode.UnprocessableEntity);
        problem.Code.ShouldBe(DomainErrorCodes.ValidationError.Value);
        problem.Detail.ShouldBe("One or more validation errors occurred.");

        problem.Errors.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("price");
        problem.Errors["price"].ShouldContain("Price cannot be negative.");
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
        ApiProblemDetails problem = await response.ReadConflictProblemAsync();

        problem.Title.ShouldBe("Conflict.");
        problem.Status.ShouldBe((int)HttpStatusCode.Conflict);
        problem.Code.ShouldBe(DomainErrorCodes.DiscontinuedProductCannotBeChanged.Value);
        problem.Detail.ShouldBe("Discontinued product cannot be changed");
        problem.Errors.ShouldBeNull();
    }
}
