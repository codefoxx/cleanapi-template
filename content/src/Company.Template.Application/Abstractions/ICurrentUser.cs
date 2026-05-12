namespace Company.Template.Application.Abstractions;

/// <summary>
/// Provides an abstraction for accessing the identity and roles of the current user.
/// </summary>
/// <remarks>
/// This interface allows the application layer to remain independent of the specific 
/// authentication infrastructure, such as HTTP context or identity providers.
/// </remarks>
public interface ICurrentUser
{
    string? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}
