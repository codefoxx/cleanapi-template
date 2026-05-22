using Company.Template.Application.Common;

namespace Company.Template.Application.Tests.Common;

public sealed class OptionTests
{
    [Fact]
    public void Some_WithValue_ReturnsOptionWithValue()
    {
        // Act
        Option<string> option = Option.Some("value");

        // Assert
        option.HasValue.ShouldBeTrue();
        option.IsNone.ShouldBeFalse();
        option.Value.ShouldBe("value");
    }

    [Fact]
    public void Some_WithNullValue_ThrowsArgumentNullException()
    {
        // Act
        Action action = () => Option.Some<string>(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void None_ReturnsOptionWithoutValue()
    {
        // Act
        Option<string> option = Option.None<string>();

        // Assert
        option.HasValue.ShouldBeFalse();
        option.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Value_OnNone_ThrowsInvalidOperationException()
    {
        // Arrange
        Option<string> option = Option.None<string>();

        // Act
        Action action = () => _ = option.Value;

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Match_WithSome_CallsSomeBranch()
    {
        // Arrange
        Option<string> option = Option.Some("value");

        // Act
        string result = option.Match(
            value => $"some:{value}",
            () => "none");

        // Assert
        result.ShouldBe("some:value");
    }

    [Fact]
    public void Match_WithNone_CallsNoneBranch()
    {
        // Arrange
        Option<string> option = Option.None<string>();

        // Act
        string result = option.Match(
            value => $"some:{value}",
            () => "none");

        // Assert
        result.ShouldBe("none");
    }

    [Fact]
    public void Match_WithNullSomeDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Option<string> option = Option.Some("value");
        Func<string, string> some = null!;

        // Act
        Action action = () => option.Match(some, () => "none");

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Match_WithNullNoneDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Option<string> option = Option.None<string>();
        Func<string> none = null!;

        // Act
        Action action = () => option.Match(value => $"some:{value}", none);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

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

    [Fact]
    public void Bind_WithSome_ReturnsBoundOption()
    {
        // Arrange
        Option<int> option = Option.Some(21);

        // Act
        Option<int> result = option.Bind(value => Option.Some(value * 2));

        // Assert
        result.HasValue.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Bind_WithNone_ReturnsNone()
    {
        // Arrange
        Option<int> option = Option.None<int>();

        // Act
        Option<int> result = option.Bind(value => Option.Some(value * 2));

        // Assert
        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Bind_WithNone_DoesNotInvokeBinder()
    {
        // Arrange
        Option<int> option = Option.None<int>();
        bool binderWasCalled = false;

        // Act
        Option<int> result = option.Bind(_ =>
        {
            binderWasCalled = true;
            return Option.Some(42);
        });

        // Assert
        result.IsNone.ShouldBeTrue();
        binderWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void Bind_WithNullDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        Option<int> option = Option.Some(21);
        Func<int, Option<int>> bind = null!;

        // Act
        Action action = () => option.Bind(bind);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Where_WithPredicateTrue_ReturnsSameOption()
    {
        // Arrange
        Option<int> option = Option.Some(21);

        // Act
        Option<int> result = option.Where(value => value > 0);

        // Assert
        result.ShouldBe(option);
    }

    [Fact]
    public void Where_WithPredicateFalse_ReturnsNone()
    {
        // Arrange
        Option<int> option = Option.Some(21);

        // Act
        Option<int> result = option.Where(value => value > 100);

        // Assert
        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Where_WithNone_DoesNotInvokePredicate()
    {
        // Arrange
        Option<int> option = Option.None<int>();
        bool predicateWasCalled = false;

        // Act
        Option<int> result = option.Where(_ =>
        {
            predicateWasCalled = true;
            return true;
        });

        // Assert
        result.IsNone.ShouldBeTrue();
        predicateWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void Where_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        Option<int> option = Option.Some(21);
        Func<int, bool> predicate = null!;

        // Act
        Action action = () => option.Where(predicate);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

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

    [Fact]
    public void OrElse_WithSome_ReturnsValue()
    {
        // Arrange
        Option<string> option = Option.Some("value");

        // Act
        string result = option.OrElse("fallback");

        // Assert
        result.ShouldBe("value");
    }

    [Fact]
    public void OrElse_WithNone_ReturnsFallback()
    {
        // Arrange
        Option<string> option = Option.None<string>();

        // Act
        string result = option.OrElse("fallback");

        // Assert
        result.ShouldBe("fallback");
    }

    [Fact]
    public void OrElseFactory_WithSome_DoesNotInvokeFallbackFactory()
    {
        // Arrange
        Option<string> option = Option.Some("value");
        bool fallbackWasCalled = false;

        // Act
        string result = option.OrElse(() =>
        {
            fallbackWasCalled = true;
            return "fallback";
        });

        // Assert
        result.ShouldBe("value");
        fallbackWasCalled.ShouldBeFalse();
    }

    [Fact]
    public void OrElseFactory_WithNone_ReturnsFallbackFactoryValue()
    {
        // Arrange
        Option<string> option = Option.None<string>();

        // Act
        string result = option.OrElse(() => "fallback");

        // Assert
        result.ShouldBe("fallback");
    }

    [Fact]
    public void OrElseFactory_WithNullFallbackFactory_ThrowsArgumentNullException()
    {
        // Arrange
        Option<string> option = Option.None<string>();
        Func<string> fallback = null!;

        // Act
        Action action = () => option.OrElse(fallback);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void FromNullable_WithNonNullReference_ReturnsSome()
    {
        // Arrange
        string? value = "value";

        // Act
        Option<string> option = Option.FromNullable(value);

        // Assert
        option.HasValue.ShouldBeTrue();
        option.Value.ShouldBe("value");
    }

    [Fact]
    public void FromNullable_WithNullReference_ReturnsNone()
    {
        // Arrange
        string? value = null;

        // Act
        Option<string> option = Option.FromNullable(value);

        // Assert
        option.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void FromNullable_WithNullableStructValue_ReturnsSome()
    {
        // Arrange
        int? value = 42;

        // Act
        Option<int> option = Option.FromNullable(value);

        // Assert
        option.HasValue.ShouldBeTrue();
        option.Value.ShouldBe(42);
    }

    [Fact]
    public void FromNullable_WithNullNullableStruct_ReturnsNone()
    {
        // Arrange
        int? value = null;

        // Act
        Option<int> option = Option.FromNullable(value);

        // Assert
        option.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void TryGetValue_WithSome_ReturnsTrueAndValue()
    {
        // Arrange
        Option<string> option = Option.Some("value");

        // Act
        bool result = option.TryGetValue(out string? value);

        // Assert
        result.ShouldBeTrue();
        value.ShouldBe("value");
    }

    [Fact]
    public void TryGetValue_WithNone_ReturnsFalseAndDefault()
    {
        // Arrange
        Option<string> option = Option.None<string>();

        // Act
        bool result = option.TryGetValue(out string? value);

        // Assert
        result.ShouldBeFalse();
        value.ShouldBeNull();
    }
}
