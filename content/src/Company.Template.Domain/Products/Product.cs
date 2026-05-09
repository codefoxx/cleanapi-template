using System;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

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

    public ProductId Id { get; }

    public ProductName Name { get; private set; }

    public Money Price { get; private set; }

    public ProductStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? DiscontinuedAt { get; private set; }

    public static Product Create(ProductName name, Money price, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(price);

        var product = new Product(ProductId.New(), name, price, createdAt);
        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id, createdAt));

        return product;
    }

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
