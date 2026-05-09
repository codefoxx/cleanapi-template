namespace Company.Template.Api.Tests;

public sealed class ApiSmokeTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public ApiSmokeTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoot_WhenApiIsRunning_ShouldReturnOk()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateProduct_WhenRequestIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        var request = new
        {
            Name = "",
            Price = 10,
            Currency = "USD"
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_WhenRequestIsValid_ShouldCreateProduct()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        var request = new
        {
            Name = "Keyboard",
            Price = 99.99m,
            Currency = "USD"
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        ProductResponse? created = await response.Content.ReadFromJsonAsync<ProductResponse>();

        created.ShouldNotBeNull();
        created.Id.ShouldNotBe(Guid.Empty);
        created.Name.ShouldBe(request.Name);
        created.Price.ShouldBe(request.Price);
        created.Currency.ShouldBe(request.Currency);
        created.Status.ShouldBe("Active");
    }

    [Fact]
    public async Task GetProductById_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        ProductResponse created = await CreateProductAsync(
            client,
            name: "Keyboard",
            price: 99.99m,
            currency: "USD");

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/products/{created.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        ProductResponse? fetched = await response.Content.ReadFromJsonAsync<ProductResponse>();

        fetched.ShouldNotBeNull();
        fetched.Id.ShouldBe(created.Id);
        fetched.Name.ShouldBe(created.Name);
        fetched.Price.ShouldBe(created.Price);
        fetched.Currency.ShouldBe(created.Currency);
        fetched.Status.ShouldBe(created.Status);
        return;

        // Local functions
        static async Task<ProductResponse> CreateProductAsync(
            HttpClient client,
            string name,
            decimal price,
            string currency)
        {
            var request = new
            {
                Name = name,
                Price = price,
                Currency = currency
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/products", request);

            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            ProductResponse? product = await response.Content.ReadFromJsonAsync<ProductResponse>();

            product.ShouldNotBeNull();

            return product;
        }
    }

    private sealed record ProductResponse(
        Guid Id,
        string Name,
        decimal Price,
        string Currency,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? DiscontinuedAt);
}
