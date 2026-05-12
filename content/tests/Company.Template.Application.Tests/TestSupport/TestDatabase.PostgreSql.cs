using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Company.Template.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Company.Template.Application.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private const string DatabaseName = "__DATABASE_NAME___application_tests";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
                                                     .WithImage("postgres:18")
                                                     .WithDatabase(DatabaseName)
                                                     .WithUsername("postgres")
                                                     .WithPassword("postgres")
                                                     .Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public async Task<ApplicationDbContext> CreateDbContextAsync(
        IDomainEventDispatcher? domainEventDispatcher = null)
    {
        var dbContext = new ApplicationDbContext(
            CreateDbContextOptions(),
            domainEventDispatcher ?? new NoOpDomainEventDispatcher());

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }

    private DbContextOptions<ApplicationDbContext> CreateDbContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseNpgsql(_container.GetConnectionString())
              .Options;
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
