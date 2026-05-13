namespace Company.Template.Application.Common;

public static class OptionalExtensions
{
    public static Option<T> ToOption<T>(this T? value)
        where T : class
    {
        return Option.FromNullable(value);
    }

    public static Option<T> ToOption<T>(this T? value)
        where T : struct
    {
        return Option.FromNullable(value);
    }
}
