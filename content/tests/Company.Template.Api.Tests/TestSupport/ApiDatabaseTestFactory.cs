using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Composition;
using Company.Template.Infrastructure.Persistence;
using Company.Template.TestSupport.Application.DomainEvents;
using Microsoft.EntityFrameworkCore;

namespace Company.Template.Api.Tests.TestSupport;

/// <summary>
///     Creates the API test host with an isolated test database.
/// </summary>
/// <remarks>
///     Use this factory for endpoint tests that exercise persistence through the real HTTP pipeline.
///     The factory receives a lightweight logical <see cref="TestDatabase" />; the expensive database
///     server container is shared outside this factory.
/// </remarks>
public sealed class ApiDatabaseTestFactory : WebApplicationFactory<CompositionAssemblyMarker>
{
    private readonly TestDatabase _database;

    public ApiDatabaseTestFactory(TestDatabase database)
    {
        _database = database;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoveApplicationDbContextRegistration(services);

            services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();
            services.AddDbContext<ApplicationDbContext>(_database.Configure);
        });
    }

    private static void RemoveApplicationDbContextRegistration(IServiceCollection services)
    {
        ServiceDescriptor? dbContextDescriptor = services.SingleOrDefault(descriptor =>
            descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

        if (dbContextDescriptor is not null)
        {
            services.Remove(dbContextDescriptor);
        }
    }
}
