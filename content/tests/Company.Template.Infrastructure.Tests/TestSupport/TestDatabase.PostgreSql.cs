using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Domain.Common;
using Company.Template.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Company.Template.Infrastructure.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
                                                     .WithImage("postgres:18")
                                                     .WithDatabase("company_template_tests")
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

    /// <summary>
    ///     Creates a context for tests that must start from a known empty database state.
    /// </summary>
    /// <remarks>
    ///     Command-style tests usually prefer this method because each test arranges its own state.
    ///     Query tests can use it once during setup and then open additional contexts with
    ///     <see cref="CreateDbContext" /> to read from the seeded database.
    /// </remarks>
    public async Task<ApplicationDbContext> CreateCleanDbContextAsync(
        IDomainEventDispatcher? domainEventDispatcher = null)
    {
        ApplicationDbContext dbContext = CreateDbContext(domainEventDispatcher);

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }

    /// <summary>
    ///     Creates a context over the current database state.
    /// </summary>
    /// <remarks>
    ///     Use this when a test setup has already seeded data that should be reused. This keeps the
    ///     reset behaviour explicit and prevents accidental loss of shared test data.
    /// </remarks>
    public ApplicationDbContext CreateDbContext(
        IDomainEventDispatcher? domainEventDispatcher = null)
    {
        return new ApplicationDbContext(
            CreateDbContextOptions(),
            domainEventDispatcher ?? new NoOpDomainEventDispatcher());
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
