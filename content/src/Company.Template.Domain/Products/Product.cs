using System.Diagnostics.CodeAnalysis;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

/// <summary>
///     An aggregate root protecting the product lifecycle and state transitions.
/// </summary>
/// <remarks>
///     The <see cref="Product" /> aggregate root manages its lifecycle from creation with
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

    /// <summary>
    ///     Gets the date and time when the product was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    ///     Gets the date and time when the product was discontinued, if applicable.
    /// </summary>
    public DateTimeOffset? DiscontinuedAt { get; private set; }

    /// <summary>
    ///     Gets the product identifier.
    /// </summary>
    public ProductId Id { get; }

    /// <summary>
    ///     Gets the product name.
    /// </summary>
    public ProductName Name { get; private set; }

    /// <summary>
    ///     Gets the product price.
    /// </summary>
    public Money Price { get; private set; }

    /// <summary>
    ///     Gets the current product status.
    /// </summary>
    public ProductStatus Status { get; private set; }

    /// <summary>
    ///     Creates a product from already validated value objects.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <param name="price">The product price.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>A new active <see cref="Product" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="name" /> or <paramref name="price" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     This method assumes that the value objects have already enforced their own invariants.
    ///     Use <see cref="TryCreate" /> when creating a product from raw input.
    /// </remarks>
    public static Product Create(ProductName name, Money price, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(price);

        Product product = new(ProductId.New(), name, price, createdAt);
        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id, createdAt));

        return product;
    }

    /// <summary>
    ///     Attempts to create a product from raw input without throwing for expected validation failures.
    /// </summary>
    /// <param name="name">The raw product name input.</param>
    /// <param name="price">The raw product price input.</param>
    /// <param name="currency">The raw currency code input.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="product">
    ///     The created product when the method returns <see langword="true" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <param name="error">
    ///     The domain error when the method returns <see langword="false" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when a valid product could be created;
    ///     otherwise <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This method composes the validation of <see cref="ProductName" />, <see cref="Money" />,
    ///     and <see cref="Currency" /> and returns the first domain error encountered.
    /// </remarks>
    public static bool TryCreate(
        string name,
        decimal price,
        string currency,
        DateTimeOffset createdAt,
        [NotNullWhen(true)] out Product? product,
        [NotNullWhen(false)] out DomainError? error)
    {
        product = null;
        error = null;

        if (!ProductName.TryCreate(name, out ProductName? productName, out error))
        {
            return false;
        }

        if (!Money.TryCreate(price, currency, out Money? money, out error))
        {
            return false;
        }

        product = Create(productName, money, createdAt);
        return true;
    }

    /// <summary>
    ///     Renames the product when the name actually changes.
    /// </summary>
    /// <param name="newName">The new product name.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="newName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when attempting to rename a discontinued product.
    /// </exception>
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
    ///     Changes the product price and records the change as a domain event.
    /// </summary>
    /// <param name="newPrice">The new product price.</param>
    /// <param name="changedAt">The timestamp of the price change.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="newPrice" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     No domain event is recorded when the new price equals the current price.
    /// </remarks>
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
    ///     Marks the product as discontinued and records the lifecycle change as a domain event.
    /// </summary>
    /// <param name="discontinuedAt">The discontinuation timestamp.</param>
    /// <remarks>
    ///     Calling this method for an already discontinued product is idempotent and does not record
    ///     another domain event.
    /// </remarks>
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
