using Company.Template.Composition.Framework;

namespace Company.Template.Api.Tests.Composition;

public sealed class FeatureCompositionTests
{
    [Fact]
    public void ComposeFeatures_AppliesQueuedDecoratorsAfterServiceModules()
    {
        ServiceCollection services = [];

        services
           .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests).Assembly)
           .ComposeFeatures(features => features
               .Decorate<TestDecoratorFeature>()
               .Add<TestServiceFeature>());

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<ITestService>()
                .Execute()
                .ShouldBe("decorated inner");
    }

    [Fact]
    public void ComposeFeatures_ThrowsWhenDecoratorFeatureIsQueuedTwice()
    {
        ServiceCollection services = [];

        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
        {
            services
               .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests).Assembly)
               .ComposeFeatures(features => features
                   .Decorate<TestDecoratorFeature>()
                   .Decorate<TestDecoratorFeature>());
        });

        exception.Message.ShouldContain("was queued more than once");
    }

    [Fact]
    public void ComposeFeatures_ThrowsWhenDecoratorModuleIsMissing()
    {
        ServiceCollection services = [];

        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
        {
            services
               .AddFeatureServicesFromAssemblies(typeof(FeatureCompositionTests).Assembly)
               .ComposeFeatures(features => features.Decorate<MissingDecoratorFeature>());
        });

        exception.Message.ShouldContain("No service decorator modules were found");
        exception.Message.ShouldContain(nameof(MissingDecoratorFeature));
    }

    public sealed class TestServiceFeature : IFeature;

    public sealed class TestDecoratorFeature : IFeature;

    public sealed class MissingDecoratorFeature : IFeature;

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
}
