namespace Company.Template.Api.Tests.Products;

internal sealed record CreateProductRequest(
    string Name,
    decimal Price,
    string Currency)
{
    public static CreateProductRequest Valid()
    {
        return new CreateProductRequest(
            Name: "Keyboard",
            Price: 99.99m,
            Currency: "USD");
    }

    public CreateProductRequest WithName(string name)
    {
        return this with { Name = name };
    }

    public CreateProductRequest WithPrice(decimal price)
    {
        return this with { Price = price };
    }

    public CreateProductRequest WithCurrency(string currency)
    {
        return this with { Currency = currency };
    }
}
