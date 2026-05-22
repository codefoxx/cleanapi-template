using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ResultTests
{
    [Fact]
    public async Task BindAsync_WithSuccess_ReturnsBoundResult()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");

        // Act
        Result<int> bound = await result.BindAsync(text => Task.FromResult(Result<int>.Success(text.Length)));

        // Assert
        bound.IsSuccess.ShouldBeTrue();
        bound.Value.ShouldBe(2);
    }

    [Fact]
    public async Task BindAsync_WithFailure_ReturnsOriginalFailure()
    {
        // Arrange
        Error error = Error.Validation("Invalid input.");
        Result<string> result = Result<string>.Failure(error);

        // Act
        Result<int> bound = await result.BindAsync(text => Task.FromResult(Result<int>.Success(text.Length)));

        // Assert
        bound.IsFailure.ShouldBeTrue();
        bound.Error.ShouldBe(error);
    }

    [Fact]
    public async Task BindAsync_WithFailure_DoesNotInvokeBinder()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Error.Validation("Invalid input."));
        bool binderWasCalled = false;

        // Act
        Result<int> bound = await result.BindAsync(_ =>
        {
            binderWasCalled = true;
            return Task.FromResult(Result<int>.Success(42));
        });

        // Assert
        bound.IsFailure.ShouldBeTrue();
        binderWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void BindAsync_WithNullDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");
        Func<string, Task<Result<int>>> bind = null!;

        // Act
        Func<Task> action = () => result.BindAsync(bind);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
