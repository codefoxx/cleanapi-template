using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ErrorTests
{
    [Fact]
    public void Unknown_WithCodeAndMessage_ReturnsUnknownError()
    {
        // Arrange

        // Act
        Error error = Error.Unknown(ErrorCode.Create("unexpected_error"), "Unexpected error.");

        // Assert
        error.Type.ShouldBe(ErrorType.Unknown);
        error.Code.Value.ShouldBe("unexpected_error");
        error.Message.ShouldBe("Unexpected error.");
    }
}
