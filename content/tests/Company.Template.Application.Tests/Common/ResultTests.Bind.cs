using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ResultTests
{
    [Fact]
    public void Bind_WithSuccess_ReturnsBoundResult()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");

        // Act
        Result<int> bound = result.Bind(text => Result<int>.Success(text.Length));

        // Assert
        bound.IsSuccess.ShouldBeTrue();
        bound.Value.ShouldBe(2);
    }

    [Fact]
    public void Bind_WithFailure_ReturnsOriginalFailure()
    {
        // Arrange
        Error error = Error.Validation("Invalid input.");
        Result<string> result = Result<string>.Failure(error);

        // Act
        Result<int> bound = result.Bind(text => Result<int>.Success(text.Length));

        // Assert
        bound.IsFailure.ShouldBeTrue();
        bound.Error.ShouldBe(error);
    }

    [Fact]
    public void Bind_WithFailure_DoesNotInvokeBinder()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Error.Validation("Invalid input."));
        bool binderWasCalled = false;

        // Act
        Result<int> bound = result.Bind(_ =>
        {
            binderWasCalled = true;
            return Result<int>.Success(42);
        });

        // Assert
        bound.IsFailure.ShouldBeTrue();
        binderWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void Bind_WithNullDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");
        Func<string, Result<int>> bind = null!;

        // Act
        Action action = () => result.Bind(bind);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
