using System.Text;
using System.Text.Json.Nodes;
using Company.Template.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;

namespace Company.Template.Api.Tests.Middleware;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WithUnexpectedException_ReturnsTrue()
    {
        // Arrange
        GlobalExceptionHandler handler = CreateHandler();
        DefaultHttpContext context = CreateHttpContext("/api/failing");
        InvalidOperationException exception = new("Sensitive internal failure.");

        // Act
        bool handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        handled.ShouldBeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_WithUnexpectedException_WritesInternalServerErrorProblemDetails()
    {
        // Arrange
        GlobalExceptionHandler handler = CreateHandler();
        DefaultHttpContext context = CreateHttpContext("/api/failing");
        InvalidOperationException exception = new("Sensitive internal failure.");

        // Act
        await handler.TryHandleAsync(context, exception, CancellationToken.None);
        JsonNode body = await ReadRequiredJsonBodyAsync(context);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        body["title"]!.GetValue<string>().ShouldBe("An unexpected error occurred.");
        body["detail"]!.GetValue<string>().ShouldBe("The server encountered an unexpected condition.");
        body["status"]!.GetValue<int>().ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnexpectedException_DoesNotLeakExceptionMessage()
    {
        // Arrange
        GlobalExceptionHandler handler = CreateHandler();
        DefaultHttpContext context = CreateHttpContext("/api/failing");
        InvalidOperationException exception = new("Sensitive internal failure.");

        // Act
        await handler.TryHandleAsync(context, exception, CancellationToken.None);
        string body = await ReadBodyAsync(context);

        // Assert
        body.ShouldNotContain("Sensitive internal failure");
        body.ShouldNotContain(nameof(InvalidOperationException));
    }

    [Fact]
    public async Task TryHandleAsync_WithUnexpectedException_UsesRequestPathAsProblemInstance()
    {
        // Arrange
        GlobalExceptionHandler handler = CreateHandler();
        DefaultHttpContext context = CreateHttpContext("/api/failing");
        InvalidOperationException exception = new("Sensitive internal failure.");

        // Act
        await handler.TryHandleAsync(context, exception, CancellationToken.None);
        JsonNode body = await ReadRequiredJsonBodyAsync(context);

        // Assert
        body["instance"]!.GetValue<string>().ShouldBe("/api/failing");
    }

    [Fact]
    public async Task TryHandleAsync_WhenResponseAlreadyStarted_ReturnsFalse()
    {
        // Arrange
        GlobalExceptionHandler handler = CreateHandler();
        DefaultHttpContext context = CreateStartedHttpContext("/api/failing");
        InvalidOperationException exception = new("Sensitive internal failure.");

        // Act
        bool handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        handled.ShouldBeFalse();
    }

    [Fact]
    public async Task TryHandleAsync_WhenResponseAlreadyStarted_DoesNotWriteProblemDetails()
    {
        // Arrange
        GlobalExceptionHandler handler = CreateHandler();
        DefaultHttpContext context = CreateStartedHttpContext("/api/failing");
        InvalidOperationException exception = new("Sensitive internal failure.");

        // Act
        await handler.TryHandleAsync(context, exception, CancellationToken.None);
        string body = await ReadBodyAsync(context);

        // Assert
        body.ShouldBeEmpty();
    }

    [Fact]
    public async Task TryHandleAsync_WhenRequestWasCancelled_ReturnsTrue()
    {
        // Arrange
        GlobalExceptionHandler handler = CreateHandler();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        DefaultHttpContext context = CreateHttpContext("/api/failing");
        context.RequestAborted = cancellation.Token;
        OperationCanceledException exception = new(cancellation.Token);

        // Act
        bool handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        handled.ShouldBeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_WhenRequestWasCancelled_DoesNotWriteProblemDetails()
    {
        // Arrange
        GlobalExceptionHandler handler = CreateHandler();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        DefaultHttpContext context = CreateHttpContext("/api/failing");
        context.RequestAborted = cancellation.Token;
        OperationCanceledException exception = new(cancellation.Token);

        // Act
        await handler.TryHandleAsync(context, exception, CancellationToken.None);
        string body = await ReadBodyAsync(context);

        // Assert
        body.ShouldBeEmpty();
    }

    private static GlobalExceptionHandler CreateHandler()
    {
        return new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
    }

    private static DefaultHttpContext CreateHttpContext(string path)
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        return context;
    }

    private static DefaultHttpContext CreateStartedHttpContext(string path)
    {
        StartedResponseFeature response = new();
        FeatureCollection features = new();
        features.Set<IHttpRequestFeature>(new HttpRequestFeature());
        features.Set<IHttpResponseFeature>(response);

        DefaultHttpContext context = new(features);
        context.Request.Path = path;

        return context;
    }

    private static async Task<JsonNode> ReadRequiredJsonBodyAsync(HttpContext context)
    {
        string body = await ReadBodyAsync(context);
        JsonNode? json = JsonNode.Parse(body);

        return json ?? throw new InvalidOperationException("Expected JSON response body.");
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using StreamReader reader = new(context.Response.Body, Encoding.UTF8, leaveOpen: true);

        return await reader.ReadToEndAsync();
    }

    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public string? ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = new MemoryStream();

        public bool HasStarted => true;

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }
    }
}
