using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    Money Price,
    ProductStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DiscontinuedAt);
