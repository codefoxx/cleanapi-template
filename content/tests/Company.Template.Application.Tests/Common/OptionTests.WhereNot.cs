using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed partial class OptionTests
{
    [Fact]
    public void WhereNot_WithPredicateFalse_ReturnsSameOption()
    {
        // Arrange
        Option<int> option = Option.Some(21);

        // Act
        Option<int> result = option.WhereNot(value => value > 100);

        // Assert
        result.ShouldBe(option);
    }

    [Fact]
    public void WhereNot_WithPredicateTrue_ReturnsNone()
    {
        // Arrange
        Option<int> option = Option.Some(21);

        // Act
        Option<int> result = option.WhereNot(value => value > 0);

        // Assert
        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void WhereNot_WithNone_DoesNotInvokePredicate()
    {
        // Arrange
        Option<int> option = Option.None<int>();
        bool predicateWasCalled = false;

        // Act
        Option<int> result = option.WhereNot(_ =>
        {
            predicateWasCalled = true;
            return true;
        });

        // Assert
        result.IsNone.ShouldBeTrue();
        predicateWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void WhereNot_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        Option<int> option = Option.Some(21);
        Func<int, bool> predicate = null!;

        // Act
        Action action = () => option.WhereNot(predicate);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
