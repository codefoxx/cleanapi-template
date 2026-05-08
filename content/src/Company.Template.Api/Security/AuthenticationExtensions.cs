using Company.Template.Api.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Company.Template.Api.Security;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddTemplateAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration
            .GetSection(AuthenticationOptions.SectionName)
            .Get<AuthenticationOptions>() ?? new AuthenticationOptions();

        services.AddSingleton(options);

        if (!options.Enabled)
        {
            return services;
        }

        if (string.IsNullOrWhiteSpace(options.Authority))
        {
            throw new InvalidOperationException("Authentication:Authority is required when authentication is enabled.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("Authentication:Audience is required when authentication is enabled.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.Authority = options.Authority;
                jwt.Audience = options.Audience;
                jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                jwt.TokenValidationParameters.RoleClaimType = options.RoleClaimType;
            });

        return services;
    }

    public static IServiceCollection AddTemplateAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration
            .GetSection(AuthenticationOptions.SectionName)
            .Get<AuthenticationOptions>() ?? new AuthenticationOptions();

        if (!options.Enabled)
        {
            services.AddAuthorization();
            return services;
        }

        services.AddAuthorization(authorization =>
        {
            authorization.AddPolicy(TemplatePolicies.ProductsRead, policy => RequireScopeOrRole(policy, TemplatePolicies.ProductsRead));
            authorization.AddPolicy(TemplatePolicies.ProductsWrite, policy => RequireScopeOrRole(policy, TemplatePolicies.ProductsWrite));
        });

        return services;
    }

    public static RouteHandlerBuilder RequireTemplatePolicy(
        this RouteHandlerBuilder builder,
        string policy,
        AuthenticationOptions authenticationOptions)
    {
        return authenticationOptions.Enabled
            ? builder.RequireAuthorization(policy)
            : builder;
    }

    private static void RequireScopeOrRole(AuthorizationPolicyBuilder policy, string requiredValue)
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.IsInRole(requiredValue) ||
            context.User.Claims.Any(claim =>
                (claim.Type is "scope" or "scp" && claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(requiredValue)) ||
                claim.Value == requiredValue));
    }
}
