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
    public const int MaxLength = 200;

    private ProductName(string value)
    {
        Value = value;
    }

    public string Value { get; }

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
