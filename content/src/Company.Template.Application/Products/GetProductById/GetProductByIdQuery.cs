namespace Company.Template.Application.Products.GetProductById;

/// <summary>
/// Query boundary for the use case that retrieves a product snapshot by external identifier.
/// Validation and not-found outcomes are handled by the use case through explicit results.
/// </summary>
public sealed record GetProductByIdQuery(Guid ProductId);
