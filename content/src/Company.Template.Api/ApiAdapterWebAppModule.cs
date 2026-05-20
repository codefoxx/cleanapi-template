//#if (auth == "Keycloak")
using Company.Template.Api.Options;
//#endif

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
        //#if (auth == "Keycloak")
        AuthenticationOptions authenticationOptions = context.App.Services
                                                             .GetRequiredService<IOptions<AuthenticationOptions>>()
                                                             .Value;

        if (authenticationOptions.Enabled)
        {
            context.App.UseAuthentication();
            context.App.UseAuthorization();
        }
        //#endif

        context.App.MapGet("/",
            () => Results.Ok(new
            {
                Service = "Company.Template.Api",
                Status = "Running"
            }));
    }
}