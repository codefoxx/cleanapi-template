using System.Text.Json.Nodes;
using Company.Template.Api.Endpoints;
using Company.Template.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Company.Template.Api.Tests.Endpoints;

public sealed class EndpointResultExtensionsTests
{
    [Fact]
    public async Task ToHttpResult_WithSuccessfulValueResult_UsesSuccessMapping()
    {
        Result<string> result = Result<string>.Success("created");

        HttpResponseCapture response = await ExecuteAsync(result.ToHttpResult(value => Results.Ok(new TestResponse(value))));

        response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        response.RequiredBody["value"]!.GetValue<string>().ShouldBe("created");
    }

    [Fact]
    public async Task ToHttpResult_WithSuccessfulResultWithoutValue_ReturnsNoContent()
    {
        Result result = Result.Success();

        HttpResponseCapture response = await ExecuteAsync(result.ToHttpResult());

        response.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
        response.Body.ShouldBeNull();
    }

    [Theory]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound, "Resource not found.")]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict, "Conflict.")]
    [InlineData(ErrorType.Unknown, StatusCodes.Status400BadRequest, "Request failed.")]
    public async Task ToHttpResult_WithFailure_ReturnsProblemResponse(
        ErrorType errorType,
        int expectedStatusCode,
        string expectedTitle)
    {
        Error error = CreateError(errorType, "example_error", "Example failure.");
        Result<string> result = Result<string>.Failure(error);

        HttpResponseCapture response = await ExecuteAsync(result.ToHttpResult(_ => Results.Ok()));

        response.StatusCode.ShouldBe(expectedStatusCode);
        response.RequiredBody["title"]!.GetValue<string>().ShouldBe(expectedTitle);
        response.RequiredBody["detail"]!.GetValue<string>().ShouldBe("Example failure.");
        response.RequiredBody["code"]!.GetValue<string>().ShouldBe("example_error");
    }

    [Fact]
    public async Task ToHttpResult_WithSingleValidationError_ReturnsValidationProblemForRequest()
    {
        Error error = Error.Validation(ErrorCode.Create("name_required"), "Name is required.");
        Result<string> result = Result<string>.Failure(error);

        HttpResponseCapture response = await ExecuteAsync(result.ToHttpResult(_ => Results.Ok()));

        response.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        response.RequiredBody["title"]!.GetValue<string>().ShouldBe("Validation failed.");
        response.RequiredBody["detail"]!.GetValue<string>().ShouldBe("Name is required.");
        response.RequiredBody["code"]!.GetValue<string>().ShouldBe("name_required");
        response.RequiredBody["errors"]!["request"]!.AsArray().Select(value => value!.GetValue<string>())
            .ShouldBe(["Name is required."]);
    }

    [Fact]
    public async Task ToHttpResult_WithValidationDetails_GroupsErrorsByTarget()
    {
        Error[] details =
        [
            Error.Validation(ErrorCode.Create("name_required"), "Name is required.", "name"),
            Error.Validation(ErrorCode.Create("name_too_short"), "Name is too short.", "name"),
            Error.Validation(ErrorCode.Create("price_invalid"), "Price must be positive.", "price")
        ];

        Error error = Error.Validation(
            ErrorCode.Create("validation_failed"),
            "One or more validation errors occurred.",
            details);

        Result<string> result = Result<string>.Failure(error);

        HttpResponseCapture response = await ExecuteAsync(result.ToHttpResult(_ => Results.Ok()));

        response.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        response.RequiredBody["errors"]!["name"]!.AsArray().Select(value => value!.GetValue<string>())
            .ShouldBe(["Name is required.", "Name is too short."]);
        response.RequiredBody["errors"]!["price"]!.AsArray().Select(value => value!.GetValue<string>())
            .ShouldBe(["Price must be positive."]);
    }

    [Fact]
    public void ToProblemResult_WithSuccessfulResult_ThrowsInvalidOperationException()
    {
        Result<string> result = Result<string>.Success("value");

        Action action = () => result.ToProblemResult();

        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public async Task ToHttpResultAsync_WithTaskResult_ReturnsMappedResponse()
    {
        Task<Result<string>> resultTask = Task.FromResult(Result<string>.Success("value"));

        HttpResponseCapture response = await ExecuteAsync(
            await resultTask.ToHttpResultAsync(value => Results.Ok(new TestResponse(value))));

        response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        response.RequiredBody["value"]!.GetValue<string>().ShouldBe("value");
    }

    private static Error CreateError(ErrorType type, string code, string message)
    {
        ErrorCode errorCode = ErrorCode.Create(code);

        return type switch
        {
            ErrorType.NotFound => Error.NotFound(errorCode, message),
            ErrorType.Conflict => Error.Conflict(errorCode, message),
            ErrorType.Unknown => Error.Unknown(errorCode, message),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static async Task<HttpResponseCapture> ExecuteAsync(IResult result)
    {
        DefaultHttpContext context = new();
        await using MemoryStream responseBody = new();
        context.Response.Body = responseBody;

        await result.ExecuteAsync(context);

        responseBody.Position = 0;
        using StreamReader reader = new(responseBody);
        string content = await reader.ReadToEndAsync();

        JsonNode? json = string.IsNullOrWhiteSpace(content)
            ? null
            : JsonNode.Parse(content);

        return new HttpResponseCapture(context.Response.StatusCode, json);
    }

    private sealed record HttpResponseCapture(int StatusCode, JsonNode? Body)
    {
        public JsonNode RequiredBody => Body ?? throw new InvalidOperationException("Expected response body.");
    }

    private sealed record TestResponse(string Value);
}
