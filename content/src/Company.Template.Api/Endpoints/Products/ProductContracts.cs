namespace Company.Template.Api.Endpoints.Products;

public sealed record CreateProductRequest(string Name, decimal Price, string Currency);

public sealed record ChangeProductPriceRequest(decimal Price, string Currency);

public sealed record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DiscontinuedAt);
