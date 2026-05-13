namespace Company.Template.Domain.Common;

/// <summary>
///     Defines a strongly-typed identifier for domain entities.
/// </summary>
/// <remarks>
///     Strongly-typed identifiers provide type safety by ensuring that IDs for different entity types
///     cannot be accidentally swapped (e.g., using a ProductId where a CustomerId is expected).
/// </remarks>
public interface IStronglyTypedId
{
    Guid Value { get; }
}
