using Company.Template.Api.Tests.Products.Contracts;
using Company.Template.Domain.Common;

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
        ApiProblemDetails problem = await response.ReadNotFoundProblemAsync();

        problem.Title.ShouldBe("Resource not found.");
        problem.Status.ShouldBe((int)HttpStatusCode.NotFound);
        problem.Code.ShouldBe(DomainErrorCodes.NotFound.Value);
        problem.Detail.ShouldBe("Product was not found.");
        problem.Errors.ShouldBeNull();
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
