using System.Reflection;
using Company.Template.Application.Abstractions;

namespace Company.Template.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddUseCasesFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }

    private static IServiceCollection AddUseCasesFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        Type[] useCaseTypes = assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false, IsPublic: true } &&
                type.GetInterfaces().Any(IsUseCaseInterface))
            .ToArray();

        foreach (Type useCaseType in useCaseTypes)
        {
            services.AddScoped(useCaseType);
        }

        return services;
    }

    private static bool IsUseCaseInterface(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        Type genericTypeDefinition = type.GetGenericTypeDefinition();

        return genericTypeDefinition == typeof(IUseCase<>) ||
               genericTypeDefinition == typeof(IUseCase<,>);
    }
}
