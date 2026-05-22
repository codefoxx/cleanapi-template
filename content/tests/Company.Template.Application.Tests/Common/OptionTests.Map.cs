using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class OptionTests
{
    [Fact]
    public void Map_WithSome_TransformsValue()
    {
        // Arrange
        Option<int> option = Option.Some(21);

        // Act
        Option<int> result = option.Map(value => value * 2);

        // Assert
        result.HasValue.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Map_WithNone_ReturnsNone()
    {
        // Arrange
        Option<int> option = Option.None<int>();

        // Act
        Option<int> result = option.Map(value => value * 2);

        // Assert
        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Map_WithNone_DoesNotInvokeMapper()
    {
        // Arrange
        Option<int> option = Option.None<int>();
        bool mapperWasCalled = false;

        // Act
        Option<int> result = option.Map(_ =>
        {
            mapperWasCalled = true;
            return 42;
        });

        // Assert
        result.IsNone.ShouldBeTrue();
        mapperWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void Map_WithNullDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Option<int> option = Option.Some(21);
        Func<int, int> map = null!;

        // Act
        Action action = () => option.Map(map);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
