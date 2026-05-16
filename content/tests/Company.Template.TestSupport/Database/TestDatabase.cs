using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Domain.Common;
using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Company.Template.TestSupport.Database;

/// <summary>
///     Represents one isolated logical database inside the shared test database server.
/// </summary>
/// <remarks>
///     The expensive database server container is owned by <see cref="TestDatabaseServer" />.
///     This type owns only one logical database inside that server and drops it when disposed.
/// </remarks>
public sealed partial class TestDatabase : IAsyncDisposable
{
    private readonly string _databaseName;
    private readonly TestDatabaseServer _server;

    private TestDatabase(
        TestDatabaseServer server,
        string databaseName)
    {
        _server = server;
        _databaseName = databaseName;
    }

    public static async Task<TestDatabase> CreateAsync(TestDatabaseServer server)
    {
        ArgumentNullException.ThrowIfNull(server);

        string databaseName = CreateDatabaseName();
        TestDatabase database = new(server, databaseName);

        await server.CreateDatabaseAsync(databaseName);

        await using ApplicationDbContext dbContext = database.CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        return database;
    }

    public async ValueTask DisposeAsync()
    {
        await _server.DropDatabaseAsync(_databaseName);
    }

    public async Task<ApplicationDbContext> CreateCleanDbContextAsync(
        IDomainEventDispatcher? domainEventDispatcher = null)
    {
        ApplicationDbContext dbContext = CreateDbContext(domainEventDispatcher);

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }

    public ApplicationDbContext CreateDbContext(
        IDomainEventDispatcher? domainEventDispatcher = null)
    {
        return new ApplicationDbContext(
            CreateDbContextOptions(),
            domainEventDispatcher ?? new NoOpDomainEventDispatcher());
    }

    public void Configure(DbContextOptionsBuilder builder)
    {
        ConfigureProvider(builder, _server.CreateConnectionString(_databaseName));
    }

    private DbContextOptions<ApplicationDbContext> CreateDbContextOptions()
    {
        DbContextOptionsBuilder<ApplicationDbContext> builder = new();

        ConfigureProvider(builder, _server.CreateConnectionString(_databaseName));

        return builder.Options;
    }

    private static string CreateDatabaseName()
    {
        return $"test_{Guid.NewGuid():N}";
    }

    static partial void ConfigureProvider(
        DbContextOptionsBuilder builder,
        string connectionString);

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
