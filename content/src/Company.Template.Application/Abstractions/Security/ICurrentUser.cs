namespace Company.Template.Application.Abstractions.Security;

/// <summary>
///     Provides an abstraction for accessing the identity and roles of the current user.
/// </summary>
/// <remarks>
///     This interface allows the application layer to remain independent of the specific
///     authentication infrastructure, such as HTTP context or identity providers.
/// </remarks>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }

    bool IsInRole(string role);
}
