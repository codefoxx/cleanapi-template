namespace Company.Template.Domain.Common;

public static class Guard
{
    public static void ThrowIfNullOrWhiteSpace(
        string? value,
        string paramName,
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                message ?? "Value cannot be null, empty, or whitespace.",
                paramName);
        }
    }
}
