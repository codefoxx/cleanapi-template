using Company.Template.Composition.Abstractions.Contexts;
using Company.Template.Composition.Abstractions.Contracts;
using Microsoft.Extensions.Configuration;

namespace Company.Template.Api.Tests.Composition;

public sealed class FeatureCompositionTests
{
    [Fact]
    public void AddFeatureServicesFromAssemblies_WithEmptyMarkers_ThrowsArgumentException()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        Action action = () => services.AddFeatureServicesFromAssemblies();

        // Assert
        ArgumentException exception = action.ShouldThrow<ArgumentException>();
        exception.Message.ShouldContain("At least one assembly is required.");
    }

    [Fact]
    public void AddFeatureServicesFromAssemblies_WithNullMarker_ThrowsArgumentNullException()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        Action action = () => services.AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests), null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void WithConfiguration_PassesConfigurationToServiceModule()
    {
        // Arrange
        ServiceCollection services = [];
        IConfiguration configuration = CreateConfiguration("composition-value");

        // Act
        services
            .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests))
            .WithConfiguration(configuration)
            .ComposeFeatures(features => features.Add<ConfigurationFeature>());

        using ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<ConfigurationValue>()
            .Value
            .ShouldBe("composition-value");
    }

    [Fact]
    public void RequireConfiguration_WithoutConfiguration_ThrowsClearInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        Action action = () => services
            .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests))
            .ComposeFeatures(features => features.Add<ConfigurationFeature>());

        // Assert
        InvalidOperationException exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("requires configuration");
        exception.Message.ShouldContain("WithConfiguration");
    }

    [Fact]
    public void ComposeFeatures_AppliesServiceModulesInDeterministicOrder()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        services
            .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests))
            .ComposeFeatures(features => features.Add<OrderedServiceFeature>());

        using ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        provider.GetServices<IOrderedModuleRegistration>()
            .Select(registration => registration.Name)
            .ShouldBe(["alpha", "beta"]);
    }

    [Fact]
    public void ComposeFeatures_AppliesDecoratorModulesInDeterministicOrder()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        services
            .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests))
            .ComposeFeatures(features => features
                .Add<OrderedDecoratorServiceFeature>()
                .Decorate<OrderedDecoratorFeature>());

        using ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<IOrderedDecoratorService>()
            .Execute()
            .ShouldBe("beta alpha inner");
    }

    [Fact]
    public void ComposeFeatures_AppliesQueuedDecoratorsAfterServiceModules()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        services
            .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests))
            .ComposeFeatures(features => features
                .Decorate<TestDecoratorFeature>()
                .Add<TestServiceFeature>());

        using ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<ITestService>()
            .Execute()
            .ShouldBe("decorated inner");
    }

    [Fact]
    public void ComposeFeatures_IgnoresDecoratorModulesForDifferentDecoratorFeature()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        services
            .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests))
            .ComposeFeatures(features => features
                .Add<TestServiceFeature>()
                .Decorate<TestDecoratorFeature>());

        using ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<ITestService>()
            .Execute()
            .ShouldBe("decorated inner");
    }

    [Fact]
    public void ComposeFeatures_ThrowsWhenDecoratorFeatureIsQueuedTwice()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        Action action = () => services
            .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests))
            .ComposeFeatures(features => features
                .Decorate<TestDecoratorFeature>()
                .Decorate<TestDecoratorFeature>());

        // Assert
        InvalidOperationException exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("was queued more than once");
    }

    [Fact]
    public void ComposeFeatures_ThrowsWhenDecoratorModuleIsMissing()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        Action action = () => services
            .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests))
            .ComposeFeatures(features => features.Decorate<MissingDecoratorFeature>());

        // Assert
        InvalidOperationException exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("No service decorator modules were found");
        exception.Message.ShouldContain(nameof(MissingDecoratorFeature));
    }

    private static IConfiguration CreateConfiguration(string value)
    {
        Dictionary<string, string?> values = new()
        {
            ["Feature:Value"] = value
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    public sealed class ConfigurationFeature : IFeature;

    public sealed class OrderedServiceFeature : IFeature;

    public sealed class OrderedDecoratorServiceFeature : IFeature;

    public sealed class OrderedDecoratorFeature : IFeature;

    public sealed class TestServiceFeature : IFeature;

    public sealed class TestDecoratorFeature : IFeature;

    public sealed class OtherDecoratorFeature : IFeature;

    public sealed class MissingDecoratorFeature : IFeature;

    public sealed record ConfigurationValue(string Value);

    public interface IOrderedModuleRegistration
    {
        string Name { get; }
    }

    public sealed record OrderedModuleRegistration(string Name) : IOrderedModuleRegistration;

    public interface IOrderedDecoratorService
    {
        string Execute();
    }

    public sealed class OrderedDecoratorService : IOrderedDecoratorService
    {
        public string Execute()
        {
            return "inner";
        }
    }

    public sealed class AlphaOrderedDecoratorService : IOrderedDecoratorService
    {
        private readonly IOrderedDecoratorService _inner;

        public AlphaOrderedDecoratorService(IOrderedDecoratorService inner)
        {
            _inner = inner;
        }

        public string Execute()
        {
            return $"alpha {_inner.Execute()}";
        }
    }

    public sealed class BetaOrderedDecoratorService : IOrderedDecoratorService
    {
        private readonly IOrderedDecoratorService _inner;

        public BetaOrderedDecoratorService(IOrderedDecoratorService inner)
        {
            _inner = inner;
        }

        public string Execute()
        {
            return $"beta {_inner.Execute()}";
        }
    }

    public interface ITestService
    {
        string Execute();
    }

    public sealed class TestService : ITestService
    {
        public string Execute()
        {
            return "inner";
        }
    }

    public sealed class TestServiceDecorator : ITestService
    {
        private readonly ITestService _inner;

        public TestServiceDecorator(ITestService inner)
        {
            _inner = inner;
        }

        public string Execute()
        {
            return $"decorated {_inner.Execute()}";
        }
    }

    public sealed class OtherTestServiceDecorator : ITestService
    {
        private readonly ITestService _inner;

        public OtherTestServiceDecorator(ITestService inner)
        {
            _inner = inner;
        }

        public string Execute()
        {
            return $"other {_inner.Execute()}";
        }
    }

    public sealed class ConfigurationModule : IFeatureServiceModule<ConfigurationFeature>
    {
        public void Register(FeatureServiceContext context)
        {
            IConfiguration configuration = context.RequireConfiguration();
            string value = configuration["Feature:Value"]
                ?? throw new InvalidOperationException("Feature value is missing.");

            context.Services.AddSingleton(new ConfigurationValue(value));
        }
    }

    public sealed class AlphaOrderedServiceModule : IFeatureServiceModule<OrderedServiceFeature>
    {
        public void Register(FeatureServiceContext context)
        {
            context.Services.AddSingleton<IOrderedModuleRegistration>(new OrderedModuleRegistration("alpha"));
        }
    }

    public sealed class BetaOrderedServiceModule : IFeatureServiceModule<OrderedServiceFeature>
    {
        public void Register(FeatureServiceContext context)
        {
            context.Services.AddSingleton<IOrderedModuleRegistration>(new OrderedModuleRegistration("beta"));
        }
    }

    public sealed class OrderedDecoratorServiceModule : IFeatureServiceModule<OrderedDecoratorServiceFeature>
    {
        public void Register(FeatureServiceContext context)
        {
            context.Services.AddScoped<IOrderedDecoratorService, OrderedDecoratorService>();
        }
    }

    public sealed class AlphaOrderedDecoratorModule :
        IFeatureServiceDecoratorModule<OrderedDecoratorServiceFeature, OrderedDecoratorFeature>
    {
        public void Decorate(FeatureServiceContext context)
        {
            context.Services.Decorate<IOrderedDecoratorService, AlphaOrderedDecoratorService>();
        }
    }

    public sealed class BetaOrderedDecoratorModule :
        IFeatureServiceDecoratorModule<OrderedDecoratorServiceFeature, OrderedDecoratorFeature>
    {
        public void Decorate(FeatureServiceContext context)
        {
            context.Services.Decorate<IOrderedDecoratorService, BetaOrderedDecoratorService>();
        }
    }

    public sealed class TestServiceModule : IFeatureServiceModule<TestServiceFeature>
    {
        public void Register(FeatureServiceContext context)
        {
            context.Services.AddScoped<ITestService, TestService>();
        }
    }

    public sealed class TestDecoratorModule :
        IFeatureServiceDecoratorModule<TestServiceFeature, TestDecoratorFeature>
    {
        public void Decorate(FeatureServiceContext context)
        {
            context.Services.Decorate<ITestService, TestServiceDecorator>();
        }
    }

    public sealed class OtherDecoratorModule :
        IFeatureServiceDecoratorModule<TestServiceFeature, OtherDecoratorFeature>
    {
        public void Decorate(FeatureServiceContext context)
        {
            context.Services.Decorate<ITestService, OtherTestServiceDecorator>();
        }
    }
}
