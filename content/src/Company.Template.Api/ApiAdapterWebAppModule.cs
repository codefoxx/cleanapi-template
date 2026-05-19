using Company.Template.Api.Options;

namespace Company.Template.Api;

/// <summary>
///     Activates pipeline behavior owned by the HTTP API adapter boundary.
/// </summary>
/// <remarks>
///     Product endpoints and other feature-specific endpoint mappings are activated through their own feature modules.
/// </remarks>
public sealed class ApiAdapterWebAppModule : IFeatureWebAppModule<ApiAdapterFeature>
{
    public void Use(FeatureWebAppContext context)
    {
        AuthenticationOptions authenticationOptions = context.App.Services
                                                             .GetRequiredService<IOptions<AuthenticationOptions>>()
                                                             .Value;

        if (authenticationOptions.Enabled)
        {
            context.App.UseAuthentication();
            context.App.UseAuthorization();
        }

        context.App.MapGet("/",
            () => Results.Ok(new
            {
                Service = "Company.Template.Api",
                Status = "Running"
            }));
    }
}
