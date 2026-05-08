using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Company.Template.Api.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .WithDatabase("company_template_api_tests")
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

    public void Configure(DbContextOptionsBuilder<ApplicationDbContext> builder)
    {
        builder.UseNpgsql(_container.GetConnectionString());
    }
}
