using System.Net;
using System.Net.Http.Json;

namespace Company.Template.Api.Tests;

public sealed class ApiSmokeTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public ApiSmokeTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Root_ReturnsOk()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_InvalidRequest_ReturnsProblemDetails()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/products", new
        {
            Name = "",
            Price = 10,
            Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetProduct_ReturnsProduct()
    {
        using var client = _factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/products", new
        {
            Name = "Keyboard",
            Price = 99.99m,
            Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/products/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
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
