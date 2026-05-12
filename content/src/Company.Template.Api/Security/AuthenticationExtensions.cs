using Company.Template.Api.Options;
using StringSplitOptions = System.StringSplitOptions;

namespace Company.Template.Api.Security;

/// <summary>
/// Composes authentication and authorization for the API hosting boundary.
/// </summary>
/// <remarks>
/// The registrations keep token validation, role/scope policy evaluation, and template-level opt-in behavior outside
/// application use cases so expected authorization decisions remain an API concern.
/// </remarks>
public static class AuthenticationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddTemplateAuthentication()
        {
            services
                .AddOptions<AuthenticationOptions>()
                .BindConfiguration(AuthenticationOptions.SectionName)
                .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Authority),
                    "Authentication:Authority is required when authentication is enabled.")
                .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Audience),
                    "Authentication:Audience is required when authentication is enabled.")
                .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.RoleClaimType),
                    "Authentication:RoleClaimType is required when authentication is enabled.")
                .ValidateOnStart();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<AuthenticationOptions>>((jwt, authentication) =>
                {
                    AuthenticationOptions options = authentication.Value;

                    jwt.Authority = options.Authority;
                    jwt.Audience = options.Audience;
                    jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        RoleClaimType = options.RoleClaimType
                    };
                });

            return services;
        }

        public IServiceCollection AddTemplateAuthorization()
        {
            services.AddAuthorization(authorization =>
            {
                authorization.AddPolicy(
                    TemplatePolicies.ProductsRead,
                    policy => RequireScopeOrRole(policy, TemplatePolicies.ProductsRead));

                authorization.AddPolicy(
                    TemplatePolicies.ProductsWrite,
                    policy => RequireScopeOrRole(policy, TemplatePolicies.ProductsWrite));
            });

            return services;
        }
    }

    public static RouteHandlerBuilder RequireTemplatePolicy(
        this RouteHandlerBuilder builder,
        string policy,
        bool authenticationEnabled)
    {
        return authenticationEnabled
            ? builder.RequireAuthorization(policy)
            : builder;
    }

    private static void RequireScopeOrRole(AuthorizationPolicyBuilder policy, string requiredValue)
    {
        policy.RequireAuthenticatedUser();

        policy.RequireAssertion(context =>
            context.User.IsInRole(requiredValue) ||
            context.User.Claims.Any(claim =>
                IsRequiredScopeClaim(claim, requiredValue) ||
                claim.Value == requiredValue));
    }

    private static bool IsRequiredScopeClaim(Claim claim, string requiredValue)
    {
        if (claim.Type is not ("scope" or "scp"))
        {
            return false;
        }

        return Enumerable.Contains(claim.Value
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), requiredValue, StringComparer.Ordinal);
    }
}
