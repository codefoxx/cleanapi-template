using Company.Template.Application.Common;
using Company.Template.Application.Common.Validation;

namespace Company.Template.Application.Tests.Common.Validation;

public sealed class ValidationResultTests
{
    [Fact]
    public void Success_WithValue_ReturnsValidResult()
    {
        // Act
        ValidationResult<string> result = ValidationResult<string>.Success("value");

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe("value");
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Success_WithNullValue_ThrowsArgumentNullException()
    {
        // Act
        Action action = () => ValidationResult<string>.Success(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Failure_WithErrors_ReturnsInvalidResult()
    {
        // Arrange
        Error[] errors =
        [
            Error.Validation(ErrorCodes.ValidationError, "Name is required.", "name")
        ];

        // Act
        ValidationResult<string> result = ValidationResult<string>.Failure(errors);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldBe(errors);
    }

    [Fact]
    public void Failure_WithEmptyErrors_ThrowsArgumentException()
    {
        // Act
        Action action = () => ValidationResult<string>.Failure([]);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        ValidationResult<string> result = ValidationResult<string>.Failure([
            Error.Validation("Invalid input.")
        ]);

        // Act
        Action action = () => _ = result.Value;

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void ToResult_WithSuccess_ReturnsSuccessfulResult()
    {
        // Arrange
        ValidationResult<string> validationResult = ValidationResult<string>.Success("value");

        // Act
        Result<string> result = validationResult.ToResult();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("value");
    }

    [Fact]
    public void ToResult_WithFailure_ReturnsValidationFailureWithDetails()
    {
        // Arrange
        Error[] errors =
        [
            Error.Validation(ErrorCodes.ValidationError, "Name is required.", "name"),
            Error.Validation(ErrorCodes.ValidationError, "Amount must be positive.", "amount")
        ];

        ValidationResult<string> validationResult = ValidationResult<string>.Failure(errors);

        // Act
        Result<string> result = validationResult.ToResult();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Type.ShouldBe(ErrorType.Validation);
        result.Error.Code.ShouldBe(ErrorCodes.ValidationError);
        result.Error.Message.ShouldBe("One or more validation errors occurred.");
        result.Error.Details.ShouldBe(errors);
    }
}
