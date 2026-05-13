namespace Company.Template.Api.Endpoints.Products;

/// <summary>
///     HTTP request contract for creating a product.
///     It reflects the transport shape accepted by the endpoint before application use cases translate input into domain
///     values.
/// </summary>
internal sealed record CreateProductRequest(string Name, decimal Price, string Currency);

/// <summary>
///     HTTP request contract for changing a product price.
///     Endpoint mapping keeps this transport model separate from the application command and domain value objects.
/// </summary>
internal sealed record ChangeProductPriceRequest(decimal Price, string Currency);

/// <summary>
///     HTTP response contract for product endpoints.
///     It exposes serialized primitives and strings at the API boundary while application and domain layers retain richer
///     types.
/// </summary>
internal sealed record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DiscontinuedAt);
