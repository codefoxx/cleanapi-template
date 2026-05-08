namespace Company.Template.Application.Products.CreateProduct;

public sealed record CreateProductCommand(string Name, decimal Price, string Currency);
