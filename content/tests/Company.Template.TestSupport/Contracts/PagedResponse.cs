namespace Company.Template.TestSupport.Contracts;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    PageResponse Page,
    TotalResponse Total);

public sealed record PageResponse(
    int Number,
    int Size,
    bool HasPrevious,
    bool HasNext);

public sealed record TotalResponse(
    int Items,
    int Pages);
