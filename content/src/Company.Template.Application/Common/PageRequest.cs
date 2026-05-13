namespace Company.Template.Application.Common;

/// <summary>
///     Represents validated paging input for application queries.
/// </summary>
/// <remarks>
///     Paging is modeled as a reusable application value object so endpoints and use cases share
///     the same defaults, limits, and validation rules. The maximum page size protects queries
///     from accidentally returning unbounded result sets.
/// </remarks>
public sealed record PageRequest
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    private PageRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int Skip => (PageNumber - 1) * PageSize;

    public static Result<PageRequest> Create(int? pageNumber, int? pageSize)
    {
        int resolvedPageNumber = pageNumber ?? DefaultPageNumber;
        int resolvedPageSize = pageSize ?? DefaultPageSize;

        return (resolvedPageNumber, resolvedPageSize) switch
        {
            (< 1, _) => Result<PageRequest>.Failure(
                Error.Validation("Page number must be greater than zero.")),

            (_, < 1) => Result<PageRequest>.Failure(
                Error.Validation("Page size must be greater than zero.")),

            (_, > MaxPageSize) => Result<PageRequest>.Failure(
                Error.Validation($"Page size cannot exceed {MaxPageSize}.")),

            _ => Result<PageRequest>.Success(
                new PageRequest(resolvedPageNumber, resolvedPageSize))
        };
    }
}
