using System.Diagnostics.CodeAnalysis;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

/// <summary>
///     A value object that protects product naming rules and prevents invalid names
///     from entering the <see cref="Product" /> aggregate.
/// </summary>
/// <remarks>
///     Use <see cref="TryCreate" /> when validating raw input that may fail as part of a normal
///     application flow. Use <see cref="Create" /> when the caller is expected to provide an already
///     valid value and invalid input should be treated as a programming error.
/// </remarks>
public sealed record ProductName
{
    /// <summary>
    ///     The maximum allowed product name length.
    /// </summary>
    public const int MaxLength = 200;

    private ProductName(string value)
    {
        Value = value;
    }

    /// <summary>
    ///     Gets the normalized product name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Creates a product name from a value that is expected to be valid.
    /// </summary>
    /// <param name="value">The product name.</param>
    /// <returns>A valid <see cref="ProductName" /> instance.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is missing, whitespace, or longer than <see cref="MaxLength" />.
    /// </exception>
    /// <remarks>
    ///     This method is intentionally strict. For expected validation failures from raw input,
    ///     prefer <see cref="TryCreate" /> so the caller can translate the returned
    ///     <see cref="DomainError" /> explicitly.
    /// </remarks>
    public static ProductName Create(string value)
    {
        return TryCreate(value, out ProductName? productName, out DomainError? error)
            ? productName
            : throw new ArgumentException(error.Message, nameof(value));
    }

    /// <summary>
    ///     Attempts to create a valid product name without throwing for expected validation failures.
    /// </summary>
    /// <param name="value">The raw product name input.</param>
    /// <param name="productName">
    ///     The created product name when the method returns <see langword="true" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <param name="error">
    ///     The domain error when the method returns <see langword="false" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when a valid product name could be created;
    ///     otherwise <see langword="false" />.
    /// </returns>
    public static bool TryCreate(
        string? value,
        [NotNullWhen(true)] out ProductName? productName,
        [NotNullWhen(false)] out DomainError? error)
    {
        DomainResult<ProductName> result = CreateProductName(value);

        productName = result.IsSuccess ? result.Value : null;
        error = result.IsSuccess ? null : result.Error;

        return result.IsSuccess;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    private static DomainResult<ProductName> CreateProductName(string? value)
    {
        return RequireValue(value)
              .Map(Normalize)
              .Bind(EnsureMaxLength)
              .Map(validValue => new ProductName(validValue));
    }

    private static DomainResult<string> RequireValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? DomainResult<string>.Failure(DomainError.Create(
                DomainErrorCodes.ProductNameRequired,
                "Product name is required."))
            : DomainResult<string>.Success(value);
    }

    private static string Normalize(string value)
    {
        return value.Trim();
    }

    private static DomainResult<string> EnsureMaxLength(string value)
    {
        return value.Length <= MaxLength
            ? DomainResult<string>.Success(value)
            : DomainResult<string>.Failure(DomainError.Create(
                DomainErrorCodes.ProductNameTooLong,
                $"Product name cannot exceed {MaxLength} characters."));
    }
}
