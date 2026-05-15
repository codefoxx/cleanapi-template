using System.Reflection;
using Company.Template.Application.Common;
using Company.Template.Domain.Common;

namespace Company.Template.Application.Tests.Common;

public sealed class ErrorTests
{
    [Fact]
    public void None_ReturnsNoneError()
    {
        // Act
        Error error = Error.None;

        // Assert
        error.Type.ShouldBe(ErrorType.None);
        error.Code.ShouldBe("none");
        error.Message.ShouldBe("No error.");
        error.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Validation_WithMessage_ReturnsValidationError()
    {
        // Act
        Error error = Error.Validation("Invalid input.");

        // Assert
        error.Type.ShouldBe(ErrorType.Validation);
        error.Code.ShouldBe("validation_error");
        error.Message.ShouldBe("Invalid input.");
        error.IsNone.ShouldBeFalse();
    }

    [Fact]
    public void Validation_WithCodeAndMessage_ReturnsValidationError()
    {
        // Act
        Error error = Error.Validation("product_name_required", "Product name is required.");

        // Assert
        error.Type.ShouldBe(ErrorType.Validation);
        error.Code.ShouldBe("product_name_required");
        error.Message.ShouldBe("Product name is required.");
    }

    [Fact]
    public void NotFound_WithMessage_ReturnsNotFoundError()
    {
        // Act
        Error error = Error.NotFound("Product was not found.");

        // Assert
        error.Type.ShouldBe(ErrorType.NotFound);
        error.Code.ShouldBe("not_found");
        error.Message.ShouldBe("Product was not found.");
    }

    [Fact]
    public void Conflict_WithMessage_ReturnsConflictError()
    {
        // Act
        Error error = Error.Conflict("Product already exists.");

        // Assert
        error.Type.ShouldBe(ErrorType.Conflict);
        error.Code.ShouldBe("conflict");
        error.Message.ShouldBe("Product already exists.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Validation_WithMissingCode_ThrowsArgumentException(string code)
    {
        // Act
        Action action = () => Error.Validation(code, "Invalid input.");

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Validation_WithMissingMessage_ThrowsArgumentException(string message)
    {
        // Act
        Action action = () => Error.Validation("validation_error", message);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void ToApplicationError_AllKnownDomainErrorCodes_AreMapped()
    {
        // Arrange
        DomainErrorCode[] codes =
        [
            .. typeof(DomainErrorCodes)
              .GetFields(BindingFlags.Public | BindingFlags.Static)
              .Where(field => field.FieldType == typeof(DomainErrorCode))
              .Select(field => (DomainErrorCode)field.GetValue(null)!)
              .Where(code => !code.IsNone)
        ];

        // Act
        Error[] errors = [.. codes.Select(code => DomainError.Create(code, "Test message.").ToApplicationError())];

        // Assert
        errors.ShouldAllBe(error => error.Type != ErrorType.Unknown);
    }
}
