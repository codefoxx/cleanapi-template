using Company.Template.Application.Common;

namespace Company.Template.Api.Endpoints;

internal static class EndpointResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return onSuccess(result.Value);
        }

        return ToProblem(result.Error);
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
