using System.Diagnostics.CodeAnalysis;
using Company.Template.Domain.Common;
using Company.Template.Domain.SharedKernel;

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

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? DiscontinuedAt { get; private set; }

    public ProductId Id { get; }

    public ProductName Name { get; private set; }

    public Money Price { get; private set; }

    public ProductStatus Status { get; private set; }

    /// <remarks>
    ///     Use this strict creation method when value objects have already enforced their own invariants.
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

    /// <remarks>
    ///     Composes product name, money, and currency validation and returns the first domain error encountered.
    ///     This keeps expected raw-input failures out of exception flow.
    /// </remarks>
    public static bool TryCreate(
        string name,
        decimal price,
        string currency,
        DateTimeOffset createdAt,
        [NotNullWhen(true)] out Product? product,
        [NotNullWhen(false)] out DomainError? error)
    {
        DomainResult<Product> result = CreateProduct(name, price, currency, createdAt);

        product = result.IsSuccess ? result.Value : null;
        error = result.IsSuccess ? null : result.Error;

        return result.IsSuccess;
    }

    public DomainResult Rename(ProductName newName)
    {
        ArgumentNullException.ThrowIfNull(newName);

        DomainResult canBeChanged = EnsureCanBeChanged();

        if (!canBeChanged.IsSuccess)
        {
            return canBeChanged;
        }

        if (Name == newName)
        {
            return DomainResult.Success();
        }

        Name = newName;

        return DomainResult.Success();
    }

    /// <remarks>
    ///     No domain event is recorded when the new price equals the current price.
    /// </remarks>
    public DomainResult ChangePrice(Money newPrice, DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(newPrice);

        DomainResult canBeChanged = EnsureCanBeChanged();

        if (!canBeChanged.IsSuccess)
        {
            return canBeChanged;
        }

        if (Price == newPrice)
        {
            return DomainResult.Success();
        }

        Money oldPrice = Price;
        Price = newPrice;

        AddDomainEvent(new ProductPriceChangedDomainEvent(Id, oldPrice, newPrice, changedAt));

        return DomainResult.Success();
    }

    /// <remarks>
    ///     Discontinuing an already discontinued product is idempotent and does not record another domain event.
    /// </remarks>
    public DomainResult Discontinue(DateTimeOffset discontinuedAt)
    {
        if (Status == ProductStatus.Discontinued)
        {
            return DomainResult.Success();
        }

        Status = ProductStatus.Discontinued;
        DiscontinuedAt = discontinuedAt;

        AddDomainEvent(new ProductDiscontinuedDomainEvent(Id, discontinuedAt));

        return DomainResult.Success();
    }

    private static DomainResult<Product> CreateProduct(
        string name,
        decimal price,
        string currency,
        DateTimeOffset createdAt)
    {
        return CreateProductName(name)
           .Bind(productName => CreateMoney(price, currency)
               .Map(money => Create(productName, money, createdAt)));
    }

    private static DomainResult<ProductName> CreateProductName(string name)
    {
        return ProductName.TryCreate(name, out ProductName? productName, out DomainError? error)
            ? DomainResult<ProductName>.Success(productName)
            : DomainResult<ProductName>.Failure(error);
    }

    private static DomainResult<Money> CreateMoney(decimal price, string currency)
    {
        return Money.TryCreate(price, currency, out Money? money, out DomainError? error)
            ? DomainResult<Money>.Success(money)
            : DomainResult<Money>.Failure(error);
    }

    private DomainResult EnsureCanBeChanged()
    {
        return Status == ProductStatus.Discontinued
            ? DomainResult.Failure(DomainError.Create(
                DomainErrorCodes.DiscontinuedProductCannotBeChanged,
                "Discontinued product cannot be changed"))
            : DomainResult.Success();
    }
}