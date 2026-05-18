using System.Reflection;

namespace Company.Template.Composition.Framework;

internal static class FeatureModuleDiscovery
{
    public static IReadOnlyList<TModule> CreateModules<TModule>(IReadOnlyList<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        return assemblies
              .Distinct()
              .SelectMany(assembly => assembly.DefinedTypes)
              .Where(type => type is { IsAbstract: false, IsInterface: false })
              .Where(type => typeof(TModule).IsAssignableFrom(type))
              .OrderBy(type => type.FullName, StringComparer.Ordinal)
              .Select(CreateModule<TModule>)
              .ToArray();
    }

    private static TModule CreateModule<TModule>(TypeInfo type)
    {
        return (TModule)(Activator.CreateInstance(type.AsType())
            ?? throw new InvalidOperationException(
                $"Feature module '{type.FullName}' must have a public parameterless constructor."));
    }
}
