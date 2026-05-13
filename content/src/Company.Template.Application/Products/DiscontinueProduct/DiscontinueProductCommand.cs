namespace Company.Template.Application.Products.DiscontinueProduct;

/// <summary>
///     Command boundary for the use case that discontinues a product.
///     The command identifies the target aggregate; lifecycle rules remain inside the domain model.
/// </summary>
public sealed record DiscontinueProductCommand(Guid ProductId);
