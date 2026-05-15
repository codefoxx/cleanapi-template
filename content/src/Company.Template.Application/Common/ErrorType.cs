namespace Company.Template.Application.Common;

/// <summary>
///     Categorizes expected application failures so outer boundaries can translate them consistently.
/// </summary>
public enum ErrorType
{
    None,

    /// <summary>The operation failed because of invalid input.</summary>
    Validation,

    /// <summary>The requested resource was not found.</summary>
    NotFound,

    /// <summary>The operation conflicted with the current state of the system.</summary>
    Conflict,

    /// <summary>
    ///     The operation failed with a known error code that has no more specific application category yet.
    /// </summary>
    Unknown
}
