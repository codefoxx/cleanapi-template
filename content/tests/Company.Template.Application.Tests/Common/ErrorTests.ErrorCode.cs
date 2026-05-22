using Company.Template.Application.Common;
using Company.Template.Domain.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ErrorTests
{
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
}
