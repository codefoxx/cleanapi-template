namespace Company.Template.Api.Options;

/// <summary>
/// API configuration for enabling JWT bearer authentication and aligning token validation with the identity provider.
/// These options are consumed at the hosting boundary rather than by application use cases or domain models.
/// </summary>
public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";
    public const string DefaultAudience = "company-template-api";
    public const string DefaultRoleClaimType = "roles";

    public bool Enabled { get; init; }
    public string Authority { get; init; } = "";
    public string Audience { get; init; } = DefaultAudience;
    public bool RequireHttpsMetadata { get; init; }
    public string RoleClaimType { get; init; } = DefaultRoleClaimType;
}
