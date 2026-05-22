using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ErrorTests
{
    [Fact]
    public void None_ReturnsNoneError()
    {
        // Arrange

        // Act
        Error error = Error.None;

        // Assert
        error.Type.ShouldBe(ErrorType.None);
        error.Code.ShouldBe(ErrorCode.None);
        error.Message.ShouldBe("No error.");
        error.IsNone.ShouldBeTrue();
    }
}
