namespace Company.Template.Api.OpenApi;

internal static class OpenApiRegistrationExtensions
{
    public static IServiceCollection AddTemplateOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            //#if (auth == "Keycloak")
            options.AddDocumentTransformer(AuthenticationOpenApiTransformers.TransformDocumentAsync);
            options.AddOperationTransformer(AuthenticationOpenApiTransformers.TransformOperationAsync);
            //#endif
        });

        //#if (auth == "Keycloak")
        services.AddSingleton<OAuth2SecuritySchemeTransformer>();
        //#endif

        return services;
    }
}