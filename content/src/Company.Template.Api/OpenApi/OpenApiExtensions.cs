using Company.Template.Api.Options;

namespace Company.Template.Api.OpenApi;

internal static class OpenApiRegistrationExtensions
{
    public static IServiceCollection AddTemplateOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                AuthenticationOptions authenticationOptions = context.ApplicationServices
                                                                     .GetRequiredService<IOptions<AuthenticationOptions>>()
                                                                     .Value;

                if (!authenticationOptions.Enabled)
                {
                    return Task.CompletedTask;
                }

                OAuth2SecuritySchemeTransformer transformer = context.ApplicationServices
                                                                     .GetRequiredService<OAuth2SecuritySchemeTransformer>();

                return transformer.TransformAsync(document, context, cancellationToken);
            });

            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                AuthenticationOptions authenticationOptions = context.ApplicationServices
                                                                     .GetRequiredService<IOptions<AuthenticationOptions>>()
                                                                     .Value;

                if (!authenticationOptions.Enabled)
                {
                    return Task.CompletedTask;
                }

                return new AuthorizationOperationTransformer()
                   .TransformAsync(operation, context, cancellationToken);
            });
        });

        services.AddSingleton<OAuth2SecuritySchemeTransformer>();

        return services;
    }
}
