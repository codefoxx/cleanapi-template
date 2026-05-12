namespace Company.Template.Application.Products.CreateProduct;

/// <summary>
/// Command boundary for the use case that creates a product from client-provided primitive values.
/// The use case coordinates conversion into domain value objects so invalid values are rejected before persistence.
/// </summary>
public sealed record CreateProductCommand(string Name, decimal Price, string Currency);
