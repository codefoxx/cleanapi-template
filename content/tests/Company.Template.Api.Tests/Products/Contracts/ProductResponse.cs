namespace Company.Template.Api.Tests.Products.Contracts;

internal sealed record ProductResponse(
    Guid Id,
    string Name,
    MoneyResponse Price,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DiscontinuedAt);

internal sealed record MoneyResponse(
    decimal Amount,
    string Currency);
