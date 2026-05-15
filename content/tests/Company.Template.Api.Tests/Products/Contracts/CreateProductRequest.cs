namespace Company.Template.Api.Tests.Products.Contracts;

internal sealed record CreateProductRequest(
    string Name,
    decimal Price,
    string Currency)
{
    public static CreateProductRequest Valid()
    {
        return new CreateProductRequest(
            "Keyboard",
            99.90m,
            "CHF");
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
