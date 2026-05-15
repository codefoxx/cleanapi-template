namespace Company.Template.Api.Endpoints.Products;

/// <summary>
///     HTTP request contract for creating a product.
/// </summary>
/// <remarks>
///     Represents the transport shape accepted by the endpoint before input is translated into an application command and
///     domain value objects.
/// </remarks>
internal sealed record CreateProductRequest(
    string Name,
    decimal Price,
    string Currency);

/// <summary>
///     HTTP request contract for changing a product price.
/// </summary>
/// <remarks>
///     Keeps the endpoint input separate from the application command and the domain <see cref="Money" /> value object.
/// </remarks>
internal sealed record ChangeProductPriceRequest(
    decimal Price,
    string Currency);

/// <summary>
///     HTTP request contract for querying products.
/// </summary>
/// <remarks>
///     Represents optional query-string filters, paging, and sorting values as received at the API boundary before they
///     are
///     translated into an application query.
/// </remarks>
internal sealed record GetProductsRequest(
    int? PageNumber,
    int? PageSize,
    string? Search,
    string? Status,
    string? Currency,
    string? SortBy,
    string? SortDirection);

/// <summary>
///     HTTP response contract for product endpoints.
/// </summary>
/// <remarks>
///     Exposes serialized primitives and strings at the API boundary while application and domain layers retain richer
///     types.
/// </remarks>
internal sealed record ProductResponse(
    Guid Id,
    string Name,
    MoneyResponse Price,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DiscontinuedAt);

/// <summary>
///     HTTP response contract for monetary values.
/// </summary>
/// <remarks>
///     Flattens the domain <see cref="Money" /> value object into a transport-friendly amount and currency representation.
/// </remarks>
internal sealed record MoneyResponse(
    decimal Amount,
    string Currency);
