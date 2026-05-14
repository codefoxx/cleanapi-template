using Company.Template.Application.Common;
using Company.Template.Domain.Common;

namespace Company.Template.Application.Tests.Common;

public sealed class DomainErrorExtensionsTests
{
    [Fact]
    public void ToApplicationError_WithProductNameRequired_ReturnsValidationError()
    {
        // Arrange
        DomainError domainError = DomainError.Create(
            DomainErrorCodes.ProductNameRequired,
            "Product name is required.");

        // Act
        Error error = domainError.ToApplicationError();

        // Assert
        error.Type.ShouldBe(ErrorType.Validation);
        error.Code.ShouldBe(DomainErrorCodes.ProductNameRequired.Value);
        error.Message.ShouldBe("Product name is required.");
    }

    [Fact]
    public void ToApplicationError_WithCurrencySymbolRequired_ReturnsValidationError()
    {
        // Arrange
        DomainError domainError = DomainError.Create(
            DomainErrorCodes.CurrencySymbolRequired,
            "Currency symbol is required.");

        // Act
        Error error = domainError.ToApplicationError();

        // Assert
        error.Type.ShouldBe(ErrorType.Validation);
        error.Code.ShouldBe(DomainErrorCodes.CurrencySymbolRequired.Value);
        error.Message.ShouldBe("Currency symbol is required.");
    }

    [Fact]
    public void ToApplicationError_WithUnknownDomainErrorCode_ReturnsUnknownError()
    {
        // Arrange
        DomainError domainError = DomainError.Create(
            DomainErrorCode.Create("some_unmapped_domain_error"),
            "Something domain-specific failed.");

        // Act
        Error error = domainError.ToApplicationError();

        // Assert
        error.Type.ShouldBe(ErrorType.Unknown);
        error.Code.ShouldBe("some_unmapped_domain_error");
        error.Message.ShouldBe("Something domain-specific failed.");
    }

    [Fact]
    public void ToApplicationError_WithNullError_ThrowsArgumentNullException()
    {
        // Act
        Action action = () => ((DomainError)null!).ToApplicationError();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
