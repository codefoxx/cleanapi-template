using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ErrorTests
{
    [Fact]
    public void Conflict_WithMessage_ReturnsConflictError()
    {
        // Act
        Error error = Error.Conflict("Product already exists.");

        // Assert
        error.Type.ShouldBe(ErrorType.Conflict);
        error.Code.ShouldBe(ErrorCodes.Conflict);
        error.Message.ShouldBe("Product already exists.");
    }

    [Fact]
    public void Conflict_WithCodeAndMessage_ReturnsConflictError()
    {
        // Act
        Error error = Error.Conflict(ErrorCodes.Conflict, "Product already exists.");

        // Assert
        error.Type.ShouldBe(ErrorType.Conflict);
        error.Code.ShouldBe(ErrorCodes.Conflict);
        error.Message.ShouldBe("Product already exists.");
    }
}
