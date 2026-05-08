namespace Company.Template.Application.Products.ChangeProductPrice;

public sealed record ChangeProductPriceCommand(Guid ProductId, decimal Price, string Currency);
