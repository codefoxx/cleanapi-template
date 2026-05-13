namespace Company.Template.Application.Products.ChangeProductPrice;

/// <summary>
///     Command boundary for the use case that changes a product price.
///     It carries primitive input from the application edge; the use case is responsible for loading the aggregate
///     and asking the domain model to protect price-related invariants.
/// </summary>
public sealed record ChangeProductPriceCommand(Guid ProductId, decimal Price, string Currency);
