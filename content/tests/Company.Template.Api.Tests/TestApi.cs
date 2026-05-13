namespace Company.Template.Api.Tests;

internal static class TestApi
{
    public static readonly Uri Root = Relative("/");
    public static readonly Uri Products = Relative("/api/products");

    public static Uri ProductById(Guid productId)
    {
        return Relative($"/api/products/{productId}");
    }

    public static async Task<ProductResponse> CreateProductAsync(
        HttpClient client,
        string name = "Keyboard",
        decimal price = 99.99m,
        string currency = "USD")
    {
        ArgumentNullException.ThrowIfNull(client);

        var request = new
        {
            Name = name,
            Price = price,
            Currency = currency
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(Products, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        ProductResponse? product = await response.Content.ReadFromJsonAsync<ProductResponse>();

        product.ShouldNotBeNull();

        return product;
    }

    private static Uri Relative(string path)
    {
        return new Uri(path, UriKind.Relative);
    }
}
