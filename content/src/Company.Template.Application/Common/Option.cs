namespace Company.Template.Application.Common;

/// <summary>
/// Represents an optional value that may or may not be present.
/// </summary>
/// <remarks>
/// Use <see cref="Option{T}"/> to explicitly handle the absence of a value without relying on nulls.
/// It encourages safe handling through functional methods like <see cref="Match{TResult}"/>.
/// </remarks>
/// <typeparam name="T">The type of the underlying value.</typeparam>
public readonly record struct Option<T>
{
    private readonly T? _value;

    private Option(T value)
    {
        _value = value;
        HasValue = true;
    }

    public bool HasValue { get; }

    public bool IsNone => !HasValue;

    /// <exception cref="InvalidOperationException">Thrown when attempting to access the value of an empty option.</exception>
    public T Value => HasValue
        ? _value!
        : throw new InvalidOperationException("Option has no value.");

    public static Option<T> Some(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Option<T>(value);
    }

    public static Option<T> None()
    {
        return default;
    }

    /// <summary>
    /// Executes the <paramref name="some"/> function if a value is present,
    /// otherwise executes the <paramref name="none"/> function.
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> some,
        Func<TResult> none)
    {
        return HasValue
            ? some(Value)
            : none();
    }

    /// <summary>
    /// Executes the <paramref name="some"/> function asynchronously if a value is present,
    /// otherwise executes the <paramref name="none"/> function.
    /// </summary>
    public Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> some,
        Func<TResult> none)
    {
        return HasValue
            ? some(Value)
            : Task.FromResult(none());
    }
}

public static class Option
{
    public static Option<T> Some<T>(T value)
    {
        return Option<T>.Some(value);
    }

    public static Option<T> None<T>()
    {
        return Option<T>.None();
    }

    /// <summary>Creates an option from a nullable value.</summary>
    public static Option<T> FromNullable<T>(T? value)
        where T : class
    {
        return value is null
            ? None<T>()
            : Some(value);
    }
}
