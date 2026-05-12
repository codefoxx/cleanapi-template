using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Company.Template.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace Company.Template.Application.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private const string DatabaseName = "__DATABASE_NAME___application_tests";

    private readonly MsSqlContainer _container = new MsSqlBuilder()
                                                .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
                                                .WithPassword("yourStrong(!)Password")
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
        string connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = DatabaseName
        }.ConnectionString;

        return new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseSqlServer(connectionString)
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
