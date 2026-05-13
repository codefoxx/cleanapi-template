namespace Company.Template.Application.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public bool HasNextPage => PageNumber < TotalPages;

    public bool HasPreviousPage => PageNumber > 1;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
