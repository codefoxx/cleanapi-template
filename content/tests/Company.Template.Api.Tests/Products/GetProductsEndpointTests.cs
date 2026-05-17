using Company.Template.Api.Tests.Products.Contracts;
using Company.Template.Domain.Common;

namespace Company.Template.Api.Tests.Products;

[Collection(DatabaseCollection.Name)]
public sealed class GetProductsEndpointTests
{
    private readonly TestDatabaseServer _server;

    public GetProductsEndpointTests(TestDatabaseServer server)
    {
        _server = server;
    }

    [Fact]
    public async Task GetProducts_WithoutQuery_ReturnsPagedProducts()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        await SeedProductsAsync(context.HttpClient);

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(ProductEndpoints.Collection);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        PagedResponse<ProductResponse> page = await response.ReadJsonAsync<PagedResponse<ProductResponse>>();

        page.Page.Number.ShouldBe(1);
        page.Page.Size.ShouldBe(20);
        page.Page.HasPrevious.ShouldBeFalse();
        page.Page.HasNext.ShouldBeFalse();

        page.Total.Items.ShouldBeGreaterThanOrEqualTo(5);
        page.Total.Pages.ShouldBeGreaterThanOrEqualTo(1);

        await response.Content.ShouldContainJsonPathsAsync(ProductJsonContracts.ProductPage);
    }

    [Fact]
    public async Task GetProducts_WithSearchFilter_ReturnsMatchingProducts()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        await SeedProductsAsync(context.HttpClient);

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.CollectionWithQuery("search=Keyboard&sortBy=name&sortDirection=asc"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        PagedResponse<ProductResponse> page = await response.ReadJsonAsync<PagedResponse<ProductResponse>>();

        page.Items.Select(product => product.Name).ShouldContain("Alpha Keyboard");
        page.Items.Select(product => product.Name).ShouldContain("Gamma Keyboard");
        page.Items.ShouldAllBe(product => product.Name.Contains("Keyboard", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetProducts_WithCurrencyFilter_ReturnsMatchingProducts()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        await SeedProductsAsync(context.HttpClient);

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.CollectionWithQuery("currency=CHF&sortBy=name&sortDirection=asc"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        PagedResponse<ProductResponse> page = await response.ReadJsonAsync<PagedResponse<ProductResponse>>();

        page.Items.ShouldAllBe(product => product.Price.Currency == "CHF");
    }

    [Fact]
    public async Task GetProducts_WithPaging_ReturnsRequestedPage()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        await SeedProductsAsync(context.HttpClient);

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.CollectionWithQuery("pageNumber=2&pageSize=2&sortBy=name&sortDirection=asc"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        PagedResponse<ProductResponse> page = await response.ReadJsonAsync<PagedResponse<ProductResponse>>();

        page.Page.Number.ShouldBe(2);
        page.Page.Size.ShouldBe(2);
        page.Items.Count.ShouldBe(2);
        page.Page.HasPrevious.ShouldBeTrue();
        page.Page.HasNext.ShouldBeTrue();
    }

    [Fact]
    public async Task GetProducts_WithInvalidStatus_ReturnsValidationProblem()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.CollectionWithQuery("status=unknown"));

        // Assert
        ApiProblemDetails problem = await response.ReadValidationProblemAsync();

        problem.Title.ShouldBe("Validation failed.");
        problem.Status.ShouldBe((int)HttpStatusCode.UnprocessableEntity);
        problem.Code.ShouldBe(DomainErrorCodes.ValidationError.Value);
        problem.Detail.ShouldBe("One or more validation errors occurred.");

        problem.Errors.ShouldNotBeNull();
        problem.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetProducts_WithInvalidSortBy_ReturnsValidationProblem()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.CollectionWithQuery("sortBy=unknown"));

        // Assert
        ApiProblemDetails problem = await response.ReadValidationProblemAsync();

        problem.Title.ShouldBe("Validation failed.");
        problem.Status.ShouldBe((int)HttpStatusCode.UnprocessableEntity);
        problem.Code.ShouldBe(DomainErrorCodes.ValidationError.Value);
        problem.Detail.ShouldBe("One or more validation errors occurred.");

        problem.Errors.ShouldNotBeNull();
        problem.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetProducts_WithInvalidPageSize_ReturnsValidationProblem()
    {
        // Arrange
        await using ApiTestContext context = await _server.CreateApiTestContextAsync();

        // Act
        HttpResponseMessage response = await context.HttpClient.SendAsync(
            ProductEndpoints.CollectionWithQuery("pageSize=0"));

        // Assert
        ApiProblemDetails problem = await response.ReadValidationProblemAsync();

        problem.Title.ShouldBe("Validation failed.");
        problem.Status.ShouldBe((int)HttpStatusCode.UnprocessableEntity);
        problem.Code.ShouldBe(DomainErrorCodes.ValidationError.Value);
        problem.Detail.ShouldBe("One or more validation errors occurred.");

        problem.Errors.ShouldNotBeNull();
        problem.Errors.ShouldNotBeEmpty();
    }

    private static async Task SeedProductsAsync(HttpClient httpClient)
    {
        await httpClient.CreateProductAsync(
            CreateProductRequest.Valid()
                                .WithName("Alpha Keyboard")
                                .WithPrice(99.90m)
                                .WithCurrency("CHF"));

        await httpClient.CreateProductAsync(
            CreateProductRequest.Valid()
                                .WithName("Beta Mouse")
                                .WithPrice(49.90m)
                                .WithCurrency("CHF"));

        await httpClient.CreateProductAsync(
            CreateProductRequest.Valid()
                                .WithName("Gamma Keyboard")
                                .WithPrice(129.00m)
                                .WithCurrency("EUR"));

        await httpClient.CreateProductAsync(
            CreateProductRequest.Valid()
                                .WithName("Delta Monitor")
                                .WithPrice(299.00m)
                                .WithCurrency("CHF"));

        await httpClient.CreateProductAsync(
            CreateProductRequest.Valid()
                                .WithName("Echo Cable")
                                .WithPrice(9.90m)
                                .WithCurrency("USD"));
    }
}
