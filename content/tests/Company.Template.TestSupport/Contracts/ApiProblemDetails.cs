namespace Company.Template.TestSupport.Contracts;

public sealed record ApiProblemDetails(
    string? Type,
    string? Title,
    int? Status,
    string? Detail,
    Dictionary<string, string[]>? Errors,
    string? Code);
