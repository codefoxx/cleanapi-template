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
public readonly record struct Option<T> where T : notnull
{
    private readonly T? _value;

    private Option(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _value = value;
        HasValue = true;
    }

    public bool HasValue { get; }

    public bool IsNone => !HasValue;

    public T Value => HasValue
        ? _value!
        : throw new InvalidOperationException("Option has no value.");

    public static Option<T> Some(T value)
    {
        return new Option<T>(value);
    }

    public static Option<T> None()
    {
        return default;
    }

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

    public Option<TResult> Map<TResult>(Func<T, TResult> map)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);

        return HasValue
            ? Option<TResult>.Some(map(Value))
            : Option<TResult>.None();
    }

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

    public Option<T> Where(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return HasValue && predicate(Value)
            ? this
            : None();
    }

    public Option<T> WhereNot(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return HasValue && !predicate(Value)
            ? this
            : None();
    }

    public T OrElse(T fallback)
    {
        return HasValue
            ? Value
            : fallback;
    }

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

    public bool TryGetValue([NotNullWhen(true)] out T value)
    {
        value = HasValue
            ? Value
            : default!;

        return HasValue;
    }
}

public static class Option
{
    public static Option<T> Some<T>(T value)
        where T : notnull
    {
        return Option<T>.Some(value);
    }

    public static Option<T> None<T>()
        where T : notnull
    {
        return Option<T>.None();
    }

    public static Option<T> FromNullable<T>(T? value)
        where T : class
    {
        return value is null
            ? None<T>()
            : Some(value);
    }

    public static Option<T> FromNullable<T>(T? value)
        where T : struct
    {
        return value.HasValue
            ? Some(value.Value)
            : None<T>();
    }
}
