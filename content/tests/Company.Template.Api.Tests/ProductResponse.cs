namespace Company.Template.Api.Tests;

internal sealed record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DiscontinuedAt);
