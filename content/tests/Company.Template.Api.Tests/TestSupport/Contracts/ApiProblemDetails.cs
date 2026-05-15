namespace Company.Template.Api.Tests.TestSupport.Contracts;

internal sealed record ApiProblemDetails(
    string? Type,
    string? Title,
    int? Status,
    string? Detail,
    Dictionary<string, string[]>? Errors,
    string? Code);
