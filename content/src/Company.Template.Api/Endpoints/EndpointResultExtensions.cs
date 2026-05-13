using Company.Template.Application.Common;

namespace Company.Template.Api.Endpoints;

/// <summary>
///     Translates explicit application results into HTTP responses at the API boundary.
/// </summary>
/// <remarks>
///     Keeping this mapping close to the endpoints lets use cases return domain-neutral failure information while the
///     transport layer decides how validation, not-found, conflict, and unexpected failures are represented over HTTP.
/// </remarks>
internal static class EndpointResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        return result is { IsSuccess: true, Value: not null }
            ? onSuccess(result.Value)
            : ToProblem(result.Error);
    }

    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    public static IResult ToHttpResult<TSource, TResponse>(
        this Result<PagedResult<TSource>> result,
        Func<TSource, TResponse> mapItem)
    {
        return result is { IsSuccess: true, Value: not null }
            ? Results.Ok(ToPagedResponse(result.Value, mapItem))
            : ToProblem(result.Error);
    }

    public static IResult ToProblemResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException(
                "A successful result cannot be converted to a problem response.");
        }

        return ToProblem(result.Error);
    }

    private static PagedResponse<TResponse> ToPagedResponse<TSource, TResponse>(
        PagedResult<TSource> result,
        Func<TSource, TResponse> mapItem)
    {
        return new PagedResponse<TResponse>(
            result.Items.Select(mapItem).ToList(),
            new PageResponse(
                result.PageNumber,
                result.PageSize,
                result.HasPreviousPage,
                result.HasNextPage),
            new TotalResponse(result.TotalCount, result.TotalPages));
    }

    private static IResult ToProblem(Error? error)
    {
        if (error is null)
        {
            return Results.Problem(title: "Unexpected error.");
        }

        return error.Type switch
        {
            ErrorType.Validation => Results.ValidationProblem(
                new Dictionary<string, string[]> { ["request"] = [error.Message] },
                title: "Validation failed.",
                statusCode: StatusCodes.Status422UnprocessableEntity),

            ErrorType.NotFound => Results.Problem(
                title: "Resource not found.",
                detail: error.Message,
                statusCode: StatusCodes.Status404NotFound),

            ErrorType.Conflict => Results.Problem(
                title: "Conflict.",
                detail: error.Message,
                statusCode: StatusCodes.Status409Conflict),

            _ => Results.Problem(
                title: "Request failed.",
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest)
        };
    }
}
