using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ErrorTests
{
    [Fact]
    public void Validation_WithMessage_ReturnsValidationError()
    {
        // Arrange

        // Act
        Error error = Error.Validation("Invalid input.");

        // Assert
        error.Type.ShouldBe(ErrorType.Validation);
        error.Code.ShouldBe(ErrorCodes.ValidationError);
        error.Message.ShouldBe("Invalid input.");
        error.IsNone.ShouldBeFalse();
    }

    [Fact]
    public void Validation_WithCodeAndMessage_ReturnsValidationError()
    {
        // Arrange

        // Act
        Error error = Error.Validation(ErrorCodes.ProductNameRequired, "Product name is required.");

        // Assert
        error.Type.ShouldBe(ErrorType.Validation);
        error.Code.ShouldBe(ErrorCodes.ProductNameRequired);
        error.Message.ShouldBe("Product name is required.");
    }

    [Fact]
    public void Validation_WithTarget_PreservesTarget()
    {
        // Arrange

        // Act
        Error error = Error.Validation(ErrorCodes.ProductNameRequired, "Product name is required.", "name");

        // Assert
        error.Type.ShouldBe(ErrorType.Validation);
        error.Code.ShouldBe(ErrorCodes.ProductNameRequired);
        error.Message.ShouldBe("Product name is required.");
        error.Target.ShouldBe("name");
    }

    [Fact]
    public void Validation_WithDetails_PreservesDetails()
    {
        // Arrange
        Error[] details =
        [
            Error.Validation(ErrorCodes.ProductNameRequired, "Product name is required.", "name"),
            Error.Validation(ErrorCodes.AmountNegative, "Amount cannot be negative.", "price")
        ];

        // Act
        Error error = Error.Validation(
            ErrorCodes.ValidationError,
            "One or more validation errors occurred.",
            details);

        // Assert
        error.Type.ShouldBe(ErrorType.Validation);
        error.Code.ShouldBe(ErrorCodes.ValidationError);
        error.Details.ShouldBe(details);
    }

    [Fact]
    public void Validation_WithNoneCode_ThrowsArgumentException()
    {
        // Arrange

        // Act
        Action action = () => Error.Validation(ErrorCode.None, "Invalid input.");

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Validation_WithMissingCode_ThrowsArgumentException(string code)
    {
        // Arrange

        // Act
        Action action = () => Error.Validation(ErrorCode.Create(code), "Invalid input.");

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Validation_WithMissingMessage_ThrowsArgumentException(string message)
    {
        // Arrange

        // Act
        Action action = () => Error.Validation(ErrorCodes.ValidationError, message);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}
