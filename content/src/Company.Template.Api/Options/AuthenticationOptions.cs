namespace Company.Template.Api.Options;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public bool Enabled { get; init; }

    public string Authority { get; init; } = "";

    public string Audience { get; init; } = "company-template-api";

    public bool RequireHttpsMetadata { get; init; }

    public string RoleClaimType { get; init; } = "roles";
}
