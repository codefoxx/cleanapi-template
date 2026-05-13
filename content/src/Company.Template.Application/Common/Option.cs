using System.Diagnostics.CodeAnalysis;

namespace Company.Template.Application.Common;

/// <summary>
///     Represents an optional value that may or may not be present.
/// </summary>
/// <remarks>
///     Use <see cref="Option{T}" /> to make absence explicit instead of returning or passing
///     <see langword="null" />.
///     Stay inside the option world with <see cref="Map{TResult}" />, <see cref="Bind{TResult}" />,
///     <see cref="Where" />, and <see cref="WhereNot" /> while you are transforming or filtering
///     optional values.
///     Use <see cref="Match{TResult}" /> when you leave the option world and translate the option
///     into another result type, such as an application <c>Result</c>, an HTTP response, or a fallback value.
/// </remarks>
/// <typeparam name="T">The type of the contained value.</typeparam>
public readonly record struct Option<T> where T : notnull
{
    private readonly T? _value;

    private Option(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _value = value;
        HasValue = true;
    }

    /// <summary>
    ///     Gets a value indicating whether this option contains a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    ///     Gets a value indicating whether this option does not contain a value.
    /// </summary>
    public bool IsNone => !HasValue;

    /// <summary>
    ///     Gets the contained value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the option does not contain a value.
    /// </exception>
    public T Value => HasValue
        ? _value!
        : throw new InvalidOperationException("Option has no value.");

    /// <summary>
    ///     Creates an option containing the specified value.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    public static Option<T> Some(T value)
    {
        return new Option<T>(value);
    }

    /// <summary>
    ///     Creates an empty option.
    /// </summary>
    public static Option<T> None()
    {
        return default;
    }

    /// <summary>
    ///     Handles both cases and returns a non-option result.
    /// </summary>
    /// <remarks>
    ///     Use <c>Match</c> when you leave the option world, for example when translating
    ///     <c>Some</c> into a successful application result and <c>None</c> into a not-found result.
    /// </remarks>
    public TResult Match<TResult>(
        Func<T, TResult> some,
        Func<TResult> none)
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);

        return HasValue
            ? some(Value)
            : none();
    }

    /// <summary>
    ///     Transforms the contained value when present and keeps <c>None</c> otherwise.
    /// </summary>
    /// <remarks>
    ///     Use <c>Map</c> when the transformation keeps you inside the option world:
    ///     <c>Option&lt;T&gt; -&gt; Option&lt;TResult&gt;</c>.
    /// </remarks>
    public Option<TResult> Map<TResult>(Func<T, TResult> map)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);

        return HasValue
            ? Option<TResult>.Some(map(Value))
            : Option<TResult>.None();
    }

    /// <summary>
    ///     Transforms the contained value using a function that already returns an option.
    /// </summary>
    /// <remarks>
    ///     Use <c>Bind</c> when the next operation may also return no value. This avoids nested
    ///     options such as <c>Option&lt;Option&lt;TResult&gt;&gt;</c>.
    /// </remarks>
    public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> bind)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(bind);

        return HasValue
            ? bind(Value)
            : Option<TResult>.None();
    }

    /// <summary>
    ///     Keeps the contained value only when it satisfies the specified predicate.
    /// </summary>
    public Option<T> Where(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return HasValue && predicate(Value)
            ? this
            : None();
    }

    /// <summary>
    ///     Keeps the contained value only when it does not satisfy the specified predicate.
    /// </summary>
    public Option<T> WhereNot(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return HasValue && !predicate(Value)
            ? this
            : None();
    }

    /// <summary>
    ///     Returns the contained value when present; otherwise returns the specified fallback value.
    /// </summary>
    public T OrElse(T fallback)
    {
        return HasValue
            ? Value
            : fallback;
    }

    /// <summary>
    ///     Returns the contained value when present; otherwise creates a fallback value.
    /// </summary>
    /// <remarks>
    ///     The fallback factory is evaluated only when the option is empty.
    /// </remarks>
    public T OrElse(Func<T> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        return HasValue
            ? Value
            : fallback();
    }

    /// <summary>
    ///     Attempts to get the contained value.
    /// </summary>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = HasValue ? Value : default;
        return HasValue;
    }
}

/// <summary>
///     Provides factory methods for creating <see cref="Option{T}" /> values.
/// </summary>
public static class Option
{
    /// <summary>
    ///     Creates an option containing the specified value.
    /// </summary>
    public static Option<T> Some<T>(T value)
        where T : notnull
    {
        return Option<T>.Some(value);
    }

    /// <summary>
    ///     Creates an empty option.
    /// </summary>
    public static Option<T> None<T>()
        where T : notnull
    {
        return Option<T>.None();
    }

    /// <summary>
    ///     Creates an option from a nullable reference value.
    /// </summary>
    public static Option<T> FromNullable<T>(T? value)
        where T : class
    {
        return value is null
            ? None<T>()
            : Some(value);
    }

    /// <summary>
    ///     Creates an option from a nullable value type.
    /// </summary>
    public static Option<T> FromNullable<T>(T? value)
        where T : struct
    {
        return value.HasValue
            ? Some(value.Value)
            : None<T>();
    }
}
