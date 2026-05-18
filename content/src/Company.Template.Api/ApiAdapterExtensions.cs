using Company.Template.Api.CurrentUser;
using Company.Template.Api.Endpoints;
using Company.Template.Api.Middleware;
using Company.Template.Api.OpenApi;
using Company.Template.Api.Options;
using Company.Template.Api.Security;
using Company.Template.Application.Abstractions.Security;

namespace Company.Template.Api;

/// <summary>
///     Exposes the API project as an HTTP adapter that can be composed by the executable entry point.
/// </summary>
/// <remarks>
///     The composition project owns application startup, while this adapter owns HTTP-specific middleware,
///     authentication metadata, OpenAPI setup, and endpoint mapping.
/// </remarks>
public static class ApiAdapterExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiAdapter()
        {
            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, HttpCurrentUser>();

            services.AddTemplateAuthentication();
            services.AddTemplateAuthorization();
            services.AddTemplateOpenApi();

            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseApiAdapter()
        {
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
            {
                app.MapOpenApi();
            }

            AuthenticationOptions authenticationOptions = app.Services
                                                         .GetRequiredService<IOptions<AuthenticationOptions>>()
                                                         .Value;

            if (authenticationOptions.Enabled)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            app.MapGet("/",
                () => Results.Ok(new
                {
                    Service = "Company.Template.Api",
                    Status = "Running"
                }));

            app.MapEndpointModulesFromAssembly<ApiAssemblyMarker>();

            return app;
        }
    }
}
