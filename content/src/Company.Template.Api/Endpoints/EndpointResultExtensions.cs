using Company.Template.Application.Common;

namespace Company.Template.Api.Endpoints;

/// <summary>
/// Translates explicit application results into HTTP responses at the API boundary.
/// </summary>
/// <remarks>
/// Keeping this mapping close to the endpoints lets use cases return domain-neutral failure information while the
/// transport layer decides how validation, not-found, conflict, and unexpected failures are represented over HTTP.
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

    private static IResult ToProblem(Error? error)
    {
        if (error is null)
        {
            return Results.Problem(title: "Unexpected error.");
        }

        return error.Code switch
        {
            "validation_error" => Results.ValidationProblem(
                new Dictionary<string, string[]> { ["request"] = [error.Message] },
                title: "Validation failed."),

            "not_found" => Results.Problem(
                title: "Resource not found.",
                detail: error.Message,
                statusCode: StatusCodes.Status404NotFound),

            "conflict" => Results.Problem(
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
