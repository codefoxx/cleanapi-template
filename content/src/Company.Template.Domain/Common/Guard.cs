namespace Company.Template.Domain.Common;

/// <summary>
///     Provides fail-fast precondition helpers for defensive programming.
/// </summary>
/// <remarks>
///     Guard clauses are used to validate method arguments and state before proceeding,
///     preventing the system from entering an inconsistent state due to invalid input.
/// </remarks>
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
