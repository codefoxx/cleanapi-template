using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Domain.Common;
using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Company.Template.Api.Tests.TestSupport;

/// <summary>
///     Creates the API test host without starting a database container.
/// </summary>
/// <remarks>
///     Use this factory for tests that only need the HTTP pipeline or endpoint metadata,
///     such as root endpoint checks and OpenAPI tests. Database-backed endpoint tests should
///     use <see cref="ApiDatabaseTestFactory" /> through <see cref="ApiTestContext" />.
/// </remarks>
public sealed class ApiLightweightTestFactory : WebApplicationFactory<ApiAssemblyMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoveApplicationDbContextRegistration(services);

            services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();

            services.AddDbContext<ApplicationDbContext>(_ =>
            {
                // Intentionally no provider. Tests using this lightweight factory must not access persistence.
            });
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
