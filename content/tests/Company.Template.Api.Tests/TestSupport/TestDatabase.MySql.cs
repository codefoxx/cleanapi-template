using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace Company.Template.Api.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithImage("mysql:9")
        .WithDatabase("company_template_api_tests")
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

    public void Configure(DbContextOptionsBuilder builder)
    {
        var connectionString = _container.GetConnectionString();
        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}
