using System.Reflection;
using System.Runtime.ExceptionServices;

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

    public static IReadOnlyList<FeatureServiceDecoratorModule> CreateServiceDecoratorModules(
        IReadOnlyList<Assembly> assemblies,
        Type decoratorFeature)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        ArgumentNullException.ThrowIfNull(decoratorFeature);

        return assemblies
              .Distinct()
              .SelectMany(assembly => assembly.DefinedTypes)
              .Where(type => type is { IsAbstract: false, IsInterface: false })
              .SelectMany(type => CreateServiceDecoratorModules(type, decoratorFeature))
              .OrderBy(module => module.ModuleType.FullName, StringComparer.Ordinal)
              .ToArray();
    }

    private static TModule CreateModule<TModule>(TypeInfo type)
    {
        return (TModule)(Activator.CreateInstance(type.AsType())
            ?? throw new InvalidOperationException(
                $"Feature module '{type.FullName}' must have a public parameterless constructor."));
    }

    private static IEnumerable<FeatureServiceDecoratorModule> CreateServiceDecoratorModules(
        TypeInfo type,
        Type decoratorFeature)
    {
        foreach (Type moduleInterface in type.ImplementedInterfaces)
        {
            if (!moduleInterface.IsGenericType ||
                moduleInterface.GetGenericTypeDefinition() != typeof(IFeatureServiceDecoratorModule<,>))
            {
                continue;
            }

            Type[] featureTypes = moduleInterface.GenericTypeArguments;

            if (featureTypes[1] != decoratorFeature)
            {
                continue;
            }

            object module = Activator.CreateInstance(type.AsType())
                ?? throw new InvalidOperationException(
                    $"Feature module '{type.FullName}' must have a public parameterless constructor.");

            MethodInfo decorateMethod = type.AsType().GetMethod(
                    nameof(IFeatureServiceDecoratorModule<IFeature, IFeature>.Decorate),
                    [typeof(FeatureServiceContext)])
                ?? throw new InvalidOperationException(
                    $"Feature decorator module '{type.FullName}' must define a public Decorate method.");

            yield return new FeatureServiceDecoratorModule(
                type.AsType(),
                featureTypes[0],
                featureTypes[1],
                module,
                decorateMethod);
        }
    }
}

internal sealed class FeatureServiceDecoratorModule
{
    private readonly MethodInfo _decorateMethod;
    private readonly object _module;

    public FeatureServiceDecoratorModule(
        Type moduleType,
        Type decoratedFeature,
        Type decoratorFeature,
        object module,
        MethodInfo decorateMethod)
    {
        ModuleType = moduleType;
        DecoratedFeature = decoratedFeature;
        DecoratorFeature = decoratorFeature;
        _module = module;
        _decorateMethod = decorateMethod;
    }

    public Type ModuleType { get; }

    public Type DecoratedFeature { get; }

    public Type DecoratorFeature { get; }

    public void Decorate(FeatureServiceContext context)
    {
        try
        {
            _decorateMethod.Invoke(_module, [context]);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
        }
    }
}
