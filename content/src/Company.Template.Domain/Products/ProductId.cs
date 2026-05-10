using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

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
