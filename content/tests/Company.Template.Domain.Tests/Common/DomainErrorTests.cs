namespace Company.Template.Domain.Tests.Common;

public sealed class DomainErrorTests
{
    [Fact]
    public void None_ReturnsNoneDomainError()
    {
        // Arrange

        // Act
        DomainError error = DomainError.None;

        // Assert
        error.IsNone.ShouldBeTrue();
        error.Code.ShouldBe(DomainErrorCode.None);
        error.Message.ShouldBe("No domain error.");
    }

    [Fact]
    public void Create_WithCodeAndMessage_ReturnsDomainError()
    {
        // Arrange

        // Act
        DomainError error = DomainError.Create(
            DomainErrorCodes.AmountNegative,
            "Amount cannot be negative.");

        // Assert
        error.IsNone.ShouldBeFalse();
        error.Code.ShouldBe(DomainErrorCodes.AmountNegative);
        error.Message.ShouldBe("Amount cannot be negative.");
    }

    [Fact]
    public void Create_WithNullCode_ThrowsArgumentNullException()
    {
        // Arrange

        // Act
        Action action = () => DomainError.Create(null!, "Amount cannot be negative.");

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNoneCode_ThrowsArgumentException()
    {
        // Arrange

        // Act
        Action action = () => DomainError.Create(
            DomainErrorCode.None,
            "Some error.");

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Create_WithMissingMessage_ThrowsArgumentException(string message)
    {
        // Arrange

        // Act
        Action action = () => DomainError.Create(DomainErrorCodes.AmountNegative, message);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}
