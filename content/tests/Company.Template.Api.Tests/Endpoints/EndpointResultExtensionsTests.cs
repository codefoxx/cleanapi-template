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
        // Arrange
        Result<string> result = Result<string>.Success("created");

        // Act
        HttpResponseCapture response = await ExecuteAsync(result.ToHttpResult(value => Results.Ok(new TestResponse(value))));

        // Assert
        response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        response.Body["value"]!.GetValue<string>().ShouldBe("created");
    }

    [Fact]
    public async Task ToHttpResult_WithSuccessfulResultWithoutValue_ReturnsNoContent()
    {
        // Arrange
        Result result = Result.Success();

        // Act
        HttpResponseCapture response = await ExecuteAsync(result.ToHttpResult());

        // Assert
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
        // Arrange
        Error error = CreateError(errorType, "example_error", "Example failure.");
        Result<string> result = Result<string>.Failure(error);

        // Act
        HttpResponseCapture response = await ExecuteAsync(result.ToHttpResult(_ => Results.Ok()));

        // Assert
        response.StatusCode.ShouldBe(expectedStatusCode);
        response.Body["title"]!.GetValue<string>().ShouldBe(expectedTitle);
        response.Body["detail"]!.GetValue<string>().ShouldBe("Example failure.");
        response.Body["code"]!.GetValue<string>().ShouldBe("example_error");
    }

    [Fact]
    public async Task ToHttpResult_WithSingleValidationError_ReturnsValidationProblemForRequest()
    {
        // Arrange
        Error error = Error.Validation(new ErrorCode("name_required"), "Name is required.");
        Result<string> result = Result<string>.Failure(error);

        // Act
        HttpResponseCapture response = await ExecuteAsync(result.ToHttpResult(_ => Results.Ok()));

        // Assert
        response.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        response.Body["title"]!.GetValue<string>().ShouldBe("Validation failed.");
        response.Body["detail"]!.GetValue<string>().ShouldBe("Name is required.");
        response.Body["code"]!.GetValue<string>().ShouldBe("name_required");
        response.Body["errors"]!["request"]!.AsArray().Select(value => value!.GetValue<string>())
            .ShouldBe(["Name is required."]);
    }

    [Fact]
    public async Task ToHttpResult_WithValidationDetails_GroupsErrorsByTarget()
    {
        // Arrange
        Error[] details =
        [
            Error.Validation(new ErrorCode("name_required"), "Name is required.", "name"),
            Error.Validation(new ErrorCode("name_too_short"), "Name is too short.", "name"),
            Error.Validation(new ErrorCode("price_invalid"), "Price must be positive.", "price")
        ];

        Error error = Error.Validation(
            new ErrorCode("validation_failed"),
            "One or more validation errors occurred.",
            details);

        Result<string> result = Result<string>.Failure(error);

        // Act
        HttpResponseCapture response = await ExecuteAsync(result.ToHttpResult(_ => Results.Ok()));

        // Assert
        response.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        response.Body["errors"]!["name"]!.AsArray().Select(value => value!.GetValue<string>())
            .ShouldBe(["Name is required.", "Name is too short."]);
        response.Body["errors"]!["price"]!.AsArray().Select(value => value!.GetValue<string>())
            .ShouldBe(["Price must be positive."]);
    }

    [Fact]
    public void ToProblemResult_WithSuccessfulResult_ThrowsInvalidOperationException()
    {
        // Arrange
        Result<string> result = Result<string>.Success("value");

        // Act
        Action action = () => result.ToProblemResult();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public async Task ToHttpResultAsync_WithTaskResult_ReturnsMappedResponse()
    {
        // Arrange
        Task<Result<string>> resultTask = Task.FromResult(Result<string>.Success("value"));

        // Act
        HttpResponseCapture response = await ExecuteAsync(
            await resultTask.ToHttpResultAsync(value => Results.Ok(new TestResponse(value))));

        // Assert
        response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        response.Body["value"]!.GetValue<string>().ShouldBe("value");
    }

    private static Error CreateError(ErrorType type, string code, string message)
    {
        return type switch
        {
            ErrorType.NotFound => Error.NotFound(new ErrorCode(code), message),
            ErrorType.Conflict => Error.Conflict(new ErrorCode(code), message),
            ErrorType.Unknown => Error.Unknown(new ErrorCode(code), message),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static async Task<HttpResponseCapture> ExecuteAsync(IResult result)
    {
        DefaultHttpContext context = new();
        await using MemoryStream body = new();
        context.Response.Body = body;

        await result.ExecuteAsync(context);

        body.Position = 0;
        using StreamReader reader = new(body);
        string content = await reader.ReadToEndAsync();

        JsonNode? json = string.IsNullOrWhiteSpace(content)
            ? null
            : JsonNode.Parse(content);

        return new HttpResponseCapture(context.Response.StatusCode, json);
    }

    private sealed record HttpResponseCapture(int StatusCode, JsonNode? Body);

    private sealed record TestResponse(string Value);
}
