using Company.Template.Api.Tests.Products.Contracts;

namespace Company.Template.Api.Tests.Products;

public sealed class GetProductsEndpointTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public GetProductsEndpointTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProducts_WithoutQuery_ReturnsPagedProducts()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();

        await SeedProductsAsync(httpClient);

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(ProductEndpoints.Collection);

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
        using HttpClient httpClient = _factory.CreateClient();

        await SeedProductsAsync(httpClient);

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
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
        using HttpClient httpClient = _factory.CreateClient();

        await SeedProductsAsync(httpClient);

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
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
        using HttpClient httpClient = _factory.CreateClient();

        await SeedProductsAsync(httpClient);

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
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
        using HttpClient httpClient = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
            ProductEndpoints.CollectionWithQuery("status=unknown"));

        // Assert
        await response.ShouldBeValidationProblemAsync("validation_error");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.ValidationProblem);
    }

    [Fact]
    public async Task GetProducts_WithInvalidSortBy_ReturnsValidationProblem()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
            ProductEndpoints.CollectionWithQuery("sortBy=unknown"));

        // Assert
        await response.ShouldBeValidationProblemAsync("validation_error");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.ValidationProblem);
    }

    [Fact]
    public async Task GetProducts_WithInvalidPageSize_ReturnsValidationProblem()
    {
        // Arrange
        using HttpClient httpClient = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.SendAsync(
            ProductEndpoints.CollectionWithQuery("pageSize=0"));

        // Assert
        await response.ShouldBeValidationProblemAsync("validation_error");
        await response.Content.ShouldContainJsonPathsAsync(ProblemJsonContracts.ValidationProblem);
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
