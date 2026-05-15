namespace Company.Template.Api.Tests.TestSupport.Contracts;

internal sealed record PagedResponse<TItem>(
    IReadOnlyList<TItem> Items,
    PageResponse Page,
    TotalResponse Total);

internal sealed record PageResponse(
    int PageNumber,
    int PageSize,
    bool HasPreviousPage,
    bool HasNextPage);

internal sealed record TotalResponse(
    int TotalCount,
    int TotalPages);
