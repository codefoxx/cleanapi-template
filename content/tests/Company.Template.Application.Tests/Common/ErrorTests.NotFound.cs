using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ErrorTests
{
    [Fact]
    public void NotFound_WithMessage_ReturnsNotFoundError()
    {
        // Act
        Error error = Error.NotFound("Product was not found.");

        // Assert
        error.Type.ShouldBe(ErrorType.NotFound);
        error.Code.ShouldBe(ErrorCodes.NotFound);
        error.Message.ShouldBe("Product was not found.");
    }

    [Fact]
    public void NotFound_WithCodeAndMessage_ReturnsNotFoundError()
    {
        // Act
        Error error = Error.NotFound(ErrorCodes.NotFound, "Product was not found.");

        // Assert
        error.Type.ShouldBe(ErrorType.NotFound);
        error.Code.ShouldBe(ErrorCodes.NotFound);
        error.Message.ShouldBe("Product was not found.");
    }
}
