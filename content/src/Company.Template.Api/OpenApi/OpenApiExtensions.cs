using Microsoft.OpenApi.Models;

namespace Company.Template.Api.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddTemplateOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT bearer token"
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
