using System.Diagnostics.CodeAnalysis;

namespace Company.Template.Domain.Common;

/// <summary>
///     Provides shared construction and validation helpers for strongly typed entity identifiers.
/// </summary>
/// <remarks>
///     Entity identifiers are thin wrappers around non-empty <see cref="Guid" /> values.
///     This helper centralizes the common validation rule while keeping concrete identifier
///     types small and type-safe.
/// </remarks>
public static class EntityId
{
    /// <summary>
    ///     Creates a new non-empty identifier value.
    /// </summary>
    /// <returns>A new version 7 <see cref="Guid" />.</returns>
    public static Guid New()
    {
        return Guid.CreateVersion7();
    }

    /// <summary>
    ///     Creates a strongly typed identifier from a value that is expected to be valid.
    /// </summary>
    /// <typeparam name="TId">The concrete strongly typed identifier.</typeparam>
    /// <param name="value">The raw identifier value.</param>
    /// <param name="create">Factory used to create the concrete identifier.</param>
    /// <param name="requiredCode">The domain error code used when the value is empty.</param>
    /// <param name="requiredMessage">The domain error message used when the value is empty.</param>
    /// <param name="parameterName">The parameter name used for the thrown exception.</param>
    /// <returns>The created strongly typed identifier.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="create" /> or <paramref name="requiredCode" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is <see cref="Guid.Empty" />.
    /// </exception>
    /// <remarks>
    ///     This method is intentionally strict. Use <see cref="TryFrom{TId}" /> when handling
    ///     raw input that may fail as part of a normal application flow.
    /// </remarks>
    public static TId From<TId>(
        Guid value,
        Func<Guid, TId> create,
        DomainErrorCode requiredCode,
        string requiredMessage,
        string parameterName)
        where TId : struct, IEntityId<TId>
    {
        if (!TryFrom(value, create, requiredCode, requiredMessage, out TId id, out DomainError? error))
        {
            throw new ArgumentException(error.Message, parameterName);
        }

        return id;
    }

    /// <summary>
    ///     Attempts to create a strongly typed identifier without throwing for expected validation failures.
    /// </summary>
    /// <typeparam name="TId">The concrete strongly typed identifier.</typeparam>
    /// <param name="value">The raw identifier value.</param>
    /// <param name="create">Factory used to create the concrete identifier.</param>
    /// <param name="requiredCode">The domain error code used when the value is empty.</param>
    /// <param name="requiredMessage">The domain error message used when the value is empty.</param>
    /// <param name="id">
    ///     The created identifier when the method returns <see langword="true" />;
    ///     otherwise the default value of <typeparamref name="TId" />.
    /// </param>
    /// <param name="error">
    ///     The domain error when the method returns <see langword="false" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when a valid identifier could be created;
    ///     otherwise <see langword="false" />.
    /// </returns>
    public static bool TryFrom<TId>(
        Guid value,
        Func<Guid, TId> create,
        DomainErrorCode requiredCode,
        string requiredMessage,
        out TId id,
        [NotNullWhen(false)] out DomainError? error)
        where TId : struct, IEntityId<TId>
    {
        ArgumentNullException.ThrowIfNull(create);
        ArgumentNullException.ThrowIfNull(requiredCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredMessage);

        if (value == Guid.Empty)
        {
            id = default;
            error = DomainError.Create(requiredCode, requiredMessage);
            return false;
        }

        id = create(value);
        error = null;
        return true;
    }
}
