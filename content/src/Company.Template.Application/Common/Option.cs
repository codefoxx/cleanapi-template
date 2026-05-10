namespace Company.Template.Application.Common;

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

    public TResult Match<TResult>(
        Func<T, TResult> some,
        Func<TResult> none)
    {
        return HasValue
            ? some(Value)
            : none();
    }

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

    public static Option<T> FromNullable<T>(T? value)
        where T : class
    {
        return value is null
            ? None<T>()
            : Some(value);
    }
}
