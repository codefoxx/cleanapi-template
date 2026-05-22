using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class ResultTests
{
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
    public void Map_WithFailure_DoesNotInvokeMapper()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Error.Validation("Invalid input."));
        bool mapperWasCalled = false;

        // Act
        Result<int> mapped = result.Map(_ =>
        {
            mapperWasCalled = true;
            return 42;
        });

        // Assert
        mapped.IsFailure.ShouldBeTrue();
        mapperWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void Map_WithNullDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Result<string> result = Result<string>.Success("ok");
        Func<string, int> map = null!;

        // Act
        Action action = () => result.Map(map);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
