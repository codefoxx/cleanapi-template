using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

/// <summary>
/// A strongly-typed identifier for the <see cref="Product"/> aggregate.
/// </summary>
/// <remarks>
/// Using a strongly-typed ID prevents accidental assignment of identifiers from different 
/// entity types, enhancing type safety across the domain and application layers.
/// </remarks>
public readonly record struct ProductId(Guid Value) : IStronglyTypedId
{
    public static ProductId New()
    {
        return new ProductId(StronglyTypedId.New());
    }

    public static ProductId From(Guid value)
    {
        return new ProductId(StronglyTypedId.EnsureNotEmpty(value, nameof(value)));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
