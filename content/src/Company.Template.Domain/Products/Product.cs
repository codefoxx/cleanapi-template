using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

/// <summary>
///     An aggregate root protecting the product lifecycle and state transitions.
/// </summary>
/// <remarks>
///     The <see cref="Product" /> aggregate root manages its entire lifecycle, from creation with
///     a name and price, through state changes like renaming and price adjustments, to
///     discontinuation. It protects invariants such as ensuring that discontinued products
///     cannot be renamed. Relevant lifecycle changes are recorded as domain events so
///     application code can react without the aggregate depending on those reactions.
/// </remarks>
public sealed class Product : AggregateRoot
{
    private Product()
    {
        Name = null!;
        Price = null!;
    }

    private Product(ProductId id, ProductName name, Money price, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Price = price;
        Status = ProductStatus.Active;
        CreatedAt = createdAt;
    }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? DiscontinuedAt { get; private set; }

    public ProductId Id { get; }

    public ProductName Name { get; private set; }

    public Money Price { get; private set; }

    public ProductStatus Status { get; private set; }

    public static Product Create(ProductName name, Money price, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(price);

        Product product = new(ProductId.New(), name, price, createdAt);
        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id, createdAt));

        return product;
    }

    /// <exception cref="InvalidOperationException">Thrown when attempting to rename a discontinued product.</exception>
    public void Rename(ProductName newName)
    {
        ArgumentNullException.ThrowIfNull(newName);

        if (Status == ProductStatus.Discontinued)
        {
            throw new InvalidOperationException("A discontinued product cannot be renamed.");
        }

        if (Name == newName)
        {
            return;
        }

        Name = newName;
    }

    /// <summary>
    ///     Changes the price of the product and records the change fact.
    /// </summary>
    public void ChangePrice(Money newPrice, DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(newPrice);

        if (Price == newPrice)
        {
            return;
        }

        Money oldPrice = Price;
        Price = newPrice;

        AddDomainEvent(new ProductPriceChangedDomainEvent(Id, oldPrice, newPrice, changedAt));
    }

    /// <summary>
    ///     Marks the product as discontinued and records the lifecycle change.
    /// </summary>
    public void Discontinue(DateTimeOffset discontinuedAt)
    {
        if (Status == ProductStatus.Discontinued)
        {
            return;
        }

        Status = ProductStatus.Discontinued;
        DiscontinuedAt = discontinuedAt;

        AddDomainEvent(new ProductDiscontinuedDomainEvent(Id, discontinuedAt));
    }
}
