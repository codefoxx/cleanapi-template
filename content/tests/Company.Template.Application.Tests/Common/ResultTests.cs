using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed class ResultTests
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
    public void Map_WithSuccess_ReturnsMappedResult()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");

        // Act
        Result<int> mapped = result.Map(text => text.Length);

        // Assert
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(2);
    }

    [Fact]
    public void Map_WithFailure_ReturnsOriginalFailure()
    {
        // Arrange
        Error error = Error.Validation("Invalid input.");
        Result<string> result = Result<string>.Failure(error);

        // Act
        Result<int> mapped = result.Map(text => text.Length);

        // Assert
        mapped.IsFailure.ShouldBeTrue();
        mapped.Error.ShouldBe(error);
    }

    [Fact]
    public void Map_WithNullDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");

        // Act
        Action action = () => result.Map<int>(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

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
    public void Bind_WithNullDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");

        // Act
        Action action = () => result.Bind<int>(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

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
    public void BindAsync_WithNullDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");

        // Act
        Func<Task> action = () => result.BindAsync<int>(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

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
}
