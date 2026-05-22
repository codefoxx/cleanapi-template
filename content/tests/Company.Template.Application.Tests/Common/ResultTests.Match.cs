using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ResultTests
{
    [Fact]
    public void Match_WithSuccess_CallsSuccessBranch()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");

        // Act
        string value = result.Match(
            text => $"success:{text}",
            error => $"failure:{error.Code}");

        // Assert
        value.ShouldBe("success:ok");
    }

    [Fact]
    public void Match_WithFailure_CallsFailureBranch()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Error.Validation("Invalid input."));

        // Act
        string value = result.Match(
            text => $"success:{text}",
            error => $"failure:{error.Code}");

        // Assert
        value.ShouldBe("failure:validation_error");
    }

    [Fact]
    public void Match_WithNullSuccessDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");
        Func<string, string> success = null!;

        // Act
        Action action = () => result.Match(success, error => $"failure:{error.Code}");

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Match_WithNullFailureDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Error.Validation("Invalid input."));
        Func<Error, string> failure = null!;

        // Act
        Action action = () => result.Match(text => $"success:{text}", failure);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
