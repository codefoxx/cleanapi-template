namespace Company.Template.Domain.Common;

/// <summary>
///     Describes a domain-level failure without deciding how the application should expose it.
/// </summary>
/// <remarks>
///     A domain error belongs to the domain model. It describes a failed invariant,
///     validation rule, or business rule in domain language.
///     The application layer may translate it into an application error, HTTP response,
///     message, or another boundary-specific representation.
/// </remarks>
public sealed record DomainError
{
    private const string NoneMessage = "No domain error.";

    private DomainError(DomainErrorCode code, string message, bool allowNone)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (!allowNone && code.IsNone)
        {
            throw new ArgumentException("A domain error must have an error code.", nameof(code));
        }

        Code = code;
        Message = message;
    }

    /// <summary>
    ///     Represents the absence of a domain error.
    /// </summary>
    public static DomainError None { get; } = new(DomainErrorCode.None, NoneMessage, true);

    /// <summary>
    ///     Gets the stable machine-readable domain error code.
    /// </summary>
    public DomainErrorCode Code { get; }

    /// <summary>
    ///     Gets a value indicating whether this value represents the absence of a domain error.
    /// </summary>
    public bool IsNone => Code.IsNone;

    /// <summary>
    ///     Gets the human-readable domain error message.
    /// </summary>
    public string Message { get; }

    public static DomainError Create(DomainErrorCode code, string message)
    {
        return new DomainError(code, message, false);
    }
}
