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
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return result.Match(
            onSuccess,
            ToProblem);
    }

    public static async Task<IResult> ToHttpResultAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, IResult> onSuccess)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(onSuccess);

        Result<T> result = await resultTask;

        return result.ToHttpResult(onSuccess);
    }

    public static IResult ToHttpResult(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Match(
            Results.NoContent,
            ToProblem);
    }

    public static async Task<IResult> ToHttpResultAsync(this Task<Result> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);

        Result result = await resultTask;

        return result.ToHttpResult();
    }

    public static IResult ToHttpResult<TSource, TResponse>(
        this Result<PagedResult<TSource>> result,
        Func<TSource, TResponse> mapItem)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(mapItem);

        return result.Match(
            pagedResult => Results.Ok(ToPagedResponse(pagedResult, mapItem)),
            ToProblem);
    }

    public static async Task<IResult> ToHttpResultAsync<TSource, TResponse>(
        this Task<Result<PagedResult<TSource>>> resultTask,
        Func<TSource, TResponse> mapItem)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(mapItem);

        Result<PagedResult<TSource>> result = await resultTask;

        return result.ToHttpResult(mapItem);
    }

    public static IResult ToProblemResult<T>(this Result<T> result)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Match(
            _ => throw new InvalidOperationException(
                "A successful result cannot be converted to a problem response."),
            ToProblem);
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

    private static IResult ToProblem(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => ToValidationProblem(error),

            ErrorType.NotFound => Results.Problem(
                title: "Resource not found.",
                detail: error.Message,
                statusCode: StatusCodes.Status404NotFound,
                extensions: CreateProblemExtensions(error)),

            ErrorType.Conflict => Results.Problem(
                title: "Conflict.",
                detail: error.Message,
                statusCode: StatusCodes.Status409Conflict,
                extensions: CreateProblemExtensions(error)),

            _ => Results.Problem(
                title: "Request failed.",
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                extensions: CreateProblemExtensions(error))
        };
    }

    private static IResult ToValidationProblem(Error error)
    {
        Dictionary<string, string[]> errors = CreateValidationErrors(error);

        return Results.ValidationProblem(
            title: "Validation failed.",
            detail: error.Message,
            statusCode: StatusCodes.Status422UnprocessableEntity,
            errors: errors,
            extensions: CreateProblemExtensions(error));
    }

    private static Dictionary<string, string[]> CreateValidationErrors(Error error)
    {
        if (error.Details is { Count: > 0 })
        {
            return error.Details
                        .GroupBy(GetTarget)
                        .ToDictionary(
                             group => group.Key,
                             group => group.Select(detail => detail.Message).ToArray());
        }

        return new Dictionary<string, string[]>
        {
            [GetTarget(error)] = [error.Message]
        };
    }

    private static string GetTarget(Error error)
    {
        return string.IsNullOrWhiteSpace(error.Target)
            ? "request"
            : error.Target;
    }

    private static Dictionary<string, object?> CreateProblemExtensions(Error error)
    {
        return new Dictionary<string, object?>
        {
            ["code"] = error.Code.Value
        };
    }
}
