using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ResultTests
{
    [Fact]
    public void NonGenericSuccess_ReturnsSuccessfulResult()
    {
        // Act
        Result result = Result.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void NonGenericFailure_WithError_ReturnsFailedResult()
    {
        // Arrange
        Error error = Error.Conflict("Conflict.");

        // Act
        Result result = Result.Failure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void NonGenericFailure_WithNoneError_ThrowsArgumentException()
    {
        // Act
        Action action = () => Result.Failure(Error.None);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void NonGenericMatch_WithSuccess_CallsSuccessBranch()
    {
        // Arrange
        Result result = Result.Success();

        // Act
        string value = result.Match(
            () => "success",
            error => $"failure:{error.Code}");

        // Assert
        value.ShouldBe("success");
    }

    [Fact]
    public void NonGenericMatch_WithFailure_CallsFailureBranch()
    {
        // Arrange
        Result result = Result.Failure(Error.Conflict("Conflict."));

        // Act
        string value = result.Match(
            () => "success",
            error => $"failure:{error.Code}");

        // Assert
        value.ShouldBe("failure:conflict");
    }

    [Fact]
    public void NonGenericMatch_WithNullSuccessDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Result result = Result.Success();
        Func<string> success = null!;

        // Act
        Action action = () => result.Match(success, error => $"failure:{error.Code}");

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void NonGenericMatch_WithNullFailureDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Result result = Result.Failure(Error.Conflict("Conflict."));
        Func<Error, string> failure = null!;

        // Act
        Action action = () => result.Match(() => "success", failure);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
