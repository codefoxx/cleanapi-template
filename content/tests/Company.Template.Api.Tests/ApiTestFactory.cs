using Company.Template.Api.Tests.TestSupport;
using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Company.Template.Infrastructure.Persistence;

namespace Company.Template.Api.Tests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly TestDatabase _database = new();

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();

        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            ServiceDescriptor? dbContextDescriptor = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();
            services.AddDbContext<ApplicationDbContext>(_database.Configure);
        });
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
