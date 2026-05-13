namespace Company.Template.Domain.Products;

/// <summary>
///     Defines the lifecycle states of a <see cref="Product" /> aggregate and its
///     allowed transitions.
/// </summary>
public enum ProductStatus
{
    Draft = 0,
    Active = 1,
    Discontinued = 2
}
