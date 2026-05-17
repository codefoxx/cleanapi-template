using Company.Template.Api.Routing;
using Company.Template.Api.Tests.Products.Contracts;
using Company.Template.Domain.Common;

namespace Company.Template.Api.Tests.Products;

[Collection(DatabaseCollection.Name)]
public sealed class CreateProductEndpointTests
{
    private readonly TestDatabaseServer _server;

    public CreateProductEndpointTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task CreateProduct_WithValidRequest_ReturnsCreatedProduct()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        CreateProductRequest request = CreateProductRequest.Valid();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendJsonAsync(
            ProductEndpoints.Create,
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        ProductResponse product = await response.ReadJsonAsync<ProductResponse>();

        response.Headers.Location.ShouldNotBeNull();
        GetLocationPath(response.Headers.Location!).ShouldBe(ApiRoutes.Products.Location(product.Id));

        product.Id.ShouldNotBe(Guid.Empty);
        product.Name.ShouldBe(request.Name);
        product.Price.Amount.ShouldBe(request.Price);
        product.Price.Currency.ShouldBe(request.Currency);
        product.Status.ShouldBe("Active");
        product.CreatedAt.ShouldNotBe(default);
        product.DiscontinuedAt.ShouldBeNull();
    }

    [Fact]
    public async Task CreateProduct_WithEmptyName_ReturnsValidationProblem()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        CreateProductRequest request = CreateProductRequest.Valid()
                                                           .WithName("");

        // Act
        HttpResponseMessage response = await context.HttpClient.SendJsonAsync(
            ProductEndpoints.Create,
            request);

        // Assert
        ApiProblemDetails problem = await response.ReadValidationProblemAsync();

        problem.Title.ShouldBe("Validation failed.");
        problem.Status.ShouldBe((int)HttpStatusCode.UnprocessableEntity);
        problem.Code.ShouldBe(DomainErrorCodes.ValidationError.Value);
        problem.Detail.ShouldBe("One or more validation errors occurred.");

        problem.Errors.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("name");
        problem.Errors["name"].ShouldContain("Product name is required.");
    }

    [Fact]
    public async Task CreateProduct_WithInvalidCurrency_ReturnsValidationProblem()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        CreateProductRequest request = CreateProductRequest.Valid()
                                                           .WithCurrency("US");

        // Act
        HttpResponseMessage response = await context.HttpClient.SendJsonAsync(
            ProductEndpoints.Create,
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
    public async Task CreateProduct_WithNegativePrice_ReturnsValidationProblem()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        CreateProductRequest request = CreateProductRequest.Valid()
                                                           .WithPrice(-1m);

        // Act
        HttpResponseMessage response = await context.HttpClient.SendJsonAsync(
            ProductEndpoints.Create,
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

    private static string GetLocationPath(Uri location)
    {
        return location.IsAbsoluteUri
            ? location.AbsolutePath
            : location.ToString();
    }
}
