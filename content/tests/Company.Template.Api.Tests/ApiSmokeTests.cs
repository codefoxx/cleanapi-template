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
        HttpResponseMessage response = await client.GetAsync(TestApi.Root);

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
        HttpResponseMessage response = await client.PostAsJsonAsync(TestApi.Products, request);

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
        HttpResponseMessage response = await client.PostAsJsonAsync(TestApi.Products, request);

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

        ProductResponse created = await TestApi.CreateProductAsync(client);

        // Act
        HttpResponseMessage response = await client.GetAsync(TestApi.ProductById(created.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        ProductResponse? fetched = await response.Content.ReadFromJsonAsync<ProductResponse>();

        fetched.ShouldNotBeNull();
        fetched.Id.ShouldBe(created.Id);
        fetched.Name.ShouldBe(created.Name);
        fetched.Price.ShouldBe(created.Price);
        fetched.Currency.ShouldBe(created.Currency);
        fetched.Status.ShouldBe(created.Status);
    }
}
