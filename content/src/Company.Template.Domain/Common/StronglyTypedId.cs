namespace Company.Template.Domain.Common;

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
