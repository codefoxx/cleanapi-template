namespace Company.Template.Api.Endpoints;

internal sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    PageResponse Page,
    TotalResponse Total);

internal sealed record PageResponse(
    int Number,
    int Size,
    bool HasPrevious,
    bool HasNext);

internal sealed record TotalResponse(
    int Items,
    int Pages);
