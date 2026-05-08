using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace Company.Template.Infrastructure.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithImage("mysql:9")
        .WithDatabase("company_template_tests")
        .WithUsername("mysql")
        .WithPassword("mysql")
        .Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public DbContextOptions<ApplicationDbContext> CreateDbContextOptions()
    {
        var connectionString = _container.GetConnectionString();

        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;
    }
}
