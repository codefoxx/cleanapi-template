using System.Diagnostics.CodeAnalysis;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

/// <summary>
///     Strongly typed identifier for a <see cref="Product" /> aggregate.
/// </summary>
/// <remarks>
///     Using a strongly typed identifier prevents accidental assignment of identifiers
///     from different entity types while keeping the underlying persistence value simple.
///     Use <see cref="TryFrom" /> for raw input that may be empty, and <see cref="From" />
///     when the caller is expected to provide a valid identifier.
/// </remarks>
public readonly record struct ProductId : IEntityId<ProductId>
{
    private ProductId(Guid value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public Guid Value { get; }

    /// <summary>
    ///     Creates a new product identifier.
    /// </summary>
    /// <returns>A new non-empty <see cref="ProductId" />.</returns>
    public static ProductId New()
    {
        return new ProductId(EntityId.New());
    }

    /// <summary>
    ///     Creates a product identifier from a value that is expected to be valid.
    /// </summary>
    /// <param name="value">The raw identifier value.</param>
    /// <returns>A valid <see cref="ProductId" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is <see cref="Guid.Empty" />.
    /// </exception>
    /// <remarks>
    ///     This method is intentionally strict. Use <see cref="TryFrom" /> when handling
    ///     raw input that may fail as part of a normal application flow.
    /// </remarks>
    public static ProductId From(Guid value)
    {
        return EntityId.From(
            value,
            static id => new ProductId(id),
            DomainErrorCodes.ProductIdRequired,
            "Product id is required.",
            nameof(value));
    }

    /// <summary>
    ///     Attempts to create a product identifier without throwing for expected validation failures.
    /// </summary>
    /// <param name="value">The raw identifier value.</param>
    /// <param name="productId">
    ///     The created product identifier when the method returns <see langword="true" />;
    ///     otherwise the default <see cref="ProductId" /> value.
    /// </param>
    /// <param name="error">
    ///     The domain error when the method returns <see langword="false" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when a valid product identifier could be created;
    ///     otherwise <see langword="false" />.
    /// </returns>
    public static bool TryFrom(
        Guid value,
        out ProductId productId,
        [NotNullWhen(false)] out DomainError? error)
    {
        return EntityId.TryFrom(
            value,
            static id => new ProductId(id),
            DomainErrorCodes.ProductIdRequired,
            "Product id is required.",
            out productId,
            out error);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
