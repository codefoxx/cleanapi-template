namespace Company.Template.Api.OpenApi;

internal static class OpenApiRegistrationExtensions
{
    public static IServiceCollection AddTemplateOpenApi(this IServiceCollection services)
    {
        //#if (auth == "None")
        services.AddOpenApi();
        //#endif
        //#if (auth == "Keycloak")
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(AuthenticationOpenApiTransformers.TransformDocumentAsync);
            options.AddOperationTransformer(AuthenticationOpenApiTransformers.TransformOperationAsync);
        });

        services.AddSingleton<OAuth2SecuritySchemeTransformer>();
        //#endif

        return services;
    }
}