namespace Company.Template.Api.Tests.Products.Contracts;

internal sealed record ChangeProductPriceRequest(
    decimal Price,
    string Currency)
{
    public static ChangeProductPriceRequest Valid()
    {
        return new ChangeProductPriceRequest(
            149.90m,
            "CHF");
    }

    public ChangeProductPriceRequest WithPrice(decimal price)
    {
        return this with { Price = price };
    }

    public ChangeProductPriceRequest WithCurrency(string currency)
    {
        return this with { Currency = currency };
    }
}
