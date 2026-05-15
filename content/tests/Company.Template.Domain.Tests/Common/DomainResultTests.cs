namespace Company.Template.Domain.Tests.Common;

public sealed class DomainResultTests
{
    [Fact]
    public void Success_WithValue_ReturnsSuccessfulDomainResult()
    {
        // Act
        DomainResult<string> result = DomainResult<string>.Success("ok");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe("ok");
        result.Error.ShouldBe(DomainError.None);
    }

    [Fact]
    public void Success_WithNullValue_ThrowsArgumentNullException()
    {
        // Act
        Action action = () => DomainResult<string>.Success(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Failure_WithError_ReturnsFailedDomainResult()
    {
        // Arrange
        DomainError error = DomainError.Create(
            DomainErrorCodes.AmountNegative,
            "Amount cannot be negative.");

        // Act
        DomainResult<string> result = DomainResult<string>.Failure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void Failure_WithNoneError_ThrowsArgumentException()
    {
        // Act
        Action action = () => DomainResult<string>.Failure(DomainError.None);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        DomainResult<string> result = DomainResult<string>.Failure(
            DomainError.Create(
                DomainErrorCodes.AmountNegative,
                "Amount cannot be negative."));

        // Act
        Action action = () => _ = result.Value;

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Match_WithSuccess_CallsSuccessBranch()
    {
        // Arrange
        DomainResult<string> result = DomainResult<string>.Success("ok");

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
        DomainResult<string> result = DomainResult<string>.Failure(
            DomainError.Create(
                DomainErrorCodes.AmountNegative,
                "Amount cannot be negative."));

        // Act
        string value = result.Match(
            text => $"success:{text}",
            error => $"failure:{error.Code.Value}");

        // Assert
        value.ShouldBe($"failure:{DomainErrorCodes.AmountNegative.Value}");
    }

    [Fact]
    public void NonGenericSuccess_ReturnsSuccessfulDomainResult()
    {
        // Act
        DomainResult result = DomainResult.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(DomainError.None);
    }

    [Fact]
    public void NonGenericFailure_WithError_ReturnsFailedDomainResult()
    {
        // Arrange
        DomainError error = DomainError.Create(
            DomainErrorCodes.AmountNegative,
            "Amount cannot be negative.");

        // Act
        DomainResult result = DomainResult.Failure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void NonGenericFailure_WithNoneError_ThrowsArgumentException()
    {
        // Act
        Action action = () => DomainResult.Failure(DomainError.None);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void NonGenericMatch_WithSuccess_CallsSuccessBranch()
    {
        // Arrange
        DomainResult result = DomainResult.Success();

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
        DomainResult result = DomainResult.Failure(
            DomainError.Create(
                DomainErrorCodes.AmountNegative,
                "Amount cannot be negative."));

        // Act
        string value = result.Match(
            () => "success",
            error => $"failure:{error.Code.Value}");

        // Assert
        value.ShouldBe($"failure:{DomainErrorCodes.AmountNegative.Value}");
    }
}
