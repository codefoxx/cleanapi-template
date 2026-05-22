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
        error.Code.ShouldBe(ErrorCode.None);
        error.Message.ShouldBe("No error.");
        error.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void ErrorCodeCreate_WithRegularValue_ReturnsCode()
    {
        // Act
        ErrorCode code = ErrorCode.Create("custom_error");

        // Assert
        code.Value.ShouldBe("custom_error");
        code.ToString().ShouldBe("custom_error");
        code.IsNone.ShouldBeFalse();
    }

    [Fact]
    public void ErrorCodeCreate_WithNoneValue_ReturnsNoneCode()
    {
        // Act
        ErrorCode code = ErrorCode.Create("none");

        // Assert
        code.ShouldBe(ErrorCode.None);
        code.IsNone.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void ErrorCodeCreate_WithMissingValue_ThrowsArgumentException(string value)
    {
        // Act
        Action action = () => ErrorCode.Create(value);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void ErrorCodeFromDomain_WithDomainCode_PreservesCodeValue()
    {
        // Arrange
        DomainErrorCode domainCode = DomainErrorCodes.ProductNameRequired;

        // Act
        ErrorCode code = ErrorCode.FromDomain(domainCode);

        // Assert
        code.Value.ShouldBe(domainCode.Value);
    }

    [Fact]
    public void ErrorCodeFromDomain_WithNullDomainCode_ThrowsArgumentNullException()
    {
        // Act
        Action action = () => ErrorCode.FromDomain(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Validation_WithMessage_ReturnsValidationError()
    {
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

    [Fact]
    public void Unknown_WithCodeAndMessage_ReturnsUnknownError()
    {
        // Act
        Error error = Error.Unknown(ErrorCode.Create("unexpected_error"), "Unexpected error.");

        // Assert
        error.Type.ShouldBe(ErrorType.Unknown);
        error.Code.Value.ShouldBe("unexpected_error");
        error.Message.ShouldBe("Unexpected error.");
    }

    [Fact]
    public void Validation_WithNoneCode_ThrowsArgumentException()
    {
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
        // Act
        Action action = () => Error.Validation(ErrorCodes.ValidationError, message);

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
