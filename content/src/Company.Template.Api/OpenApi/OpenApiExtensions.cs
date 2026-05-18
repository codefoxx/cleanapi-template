namespace Company.Template.Api.OpenApi;

internal static class OpenApiRegistrationExtensions
{
    public static IServiceCollection AddTemplateOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(AuthenticationOpenApiTransformers.TransformDocumentAsync);
            options.AddOperationTransformer(AuthenticationOpenApiTransformers.TransformOperationAsync);
        });

        services.AddSingleton<OAuth2SecuritySchemeTransformer>();

        return services;
    }
}
