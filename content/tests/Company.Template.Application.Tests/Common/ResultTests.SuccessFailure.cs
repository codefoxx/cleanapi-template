using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ResultTests
{
    [Fact]
    public void Success_WithValue_ReturnsSuccessfulResult()
    {
        // Act
        Result<string> result = Result<string>.Success("ok");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe("ok");
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Success_WithNullValue_ThrowsArgumentNullException()
    {
        // Act
        Action action = () => Result<string>.Success(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Failure_WithError_ReturnsFailedResult()
    {
        // Arrange
        Error error = Error.Validation("Invalid input.");

        // Act
        Result<string> result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void Failure_WithNoneError_ThrowsArgumentException()
    {
        // Act
        Action action = () => Result<string>.Failure(Error.None);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Error.Validation("Invalid input."));

        // Act
        Action action = () => _ = result.Value;

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }
}
