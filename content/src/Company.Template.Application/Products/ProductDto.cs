using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    ProductStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DiscontinuedAt);
