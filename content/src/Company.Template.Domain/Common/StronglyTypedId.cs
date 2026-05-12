namespace Company.Template.Domain.Common;

/// <summary>
/// Provides utility methods for creating and validating strongly-typed identifiers.
/// </summary>
/// <remarks>
/// This class centralizes the generation of unique identifiers, preferring Guid v7 
/// to improve database index performance while maintaining global uniqueness.
/// </remarks>
public static class StronglyTypedId
{
    public static Guid New()
    {
        return Guid.CreateVersion7();
    }

    public static Guid EnsureNotEmpty(Guid value, string parameterName)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("Id cannot be empty.", parameterName)
            : value;
    }
}
