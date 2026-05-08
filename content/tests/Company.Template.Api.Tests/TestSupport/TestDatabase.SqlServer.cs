using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Company.Template.Api.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
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
        builder.UseSqlServer(_container.GetConnectionString());
    }
}
