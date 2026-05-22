using System.Reflection;
using Company.Template.Application.Common;
using Company.Template.Domain.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ErrorTests
{
    [Fact]
    public void ToApplicationError_AllKnownDomainErrorCodes_AreMapped()
    {
        // Arrange
        DomainErrorCode[] codes =
        [
            .. typeof(DomainErrorCodes)
              .GetFields(BindingFlags.Public | BindingFlags.Static)
              .Where(field => field.FieldType == typeof(DomainErrorCode))
              .Select(field => (DomainErrorCode)field.GetValue(null)!)
              .Where(code => !code.IsNone)
        ];

        // Act
        Error[] errors = [.. codes.Select(code => DomainError.Create(code, "Test message.").ToApplicationError())];

        // Assert
        errors.ShouldAllBe(error => error.Type != ErrorType.Unknown);
    }
}
