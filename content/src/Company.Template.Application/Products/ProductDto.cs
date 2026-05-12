using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

/// <summary>
/// Application-facing product snapshot returned by product use cases.
/// It gives application callers a stable output shape without exposing the aggregate itself
/// or forcing API contracts to depend directly on domain entities.
/// </summary>
public sealed record ProductDto(
    Guid Id,
    string Name,
    Money Price,
    ProductStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DiscontinuedAt);
