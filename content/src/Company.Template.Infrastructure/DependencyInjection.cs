using Company.Template.Application.Abstractions;
using Company.Template.Infrastructure.DomainEvents;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Time;

namespace Company.Template.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services.AddTemplateDatabase(configuration);
            services.AddDbContextAbstractions<ApplicationDbContext>();

            services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();
            services.AddSingleton<IClock, SystemClock>();

            return services;
        }

        private IServiceCollection AddDbContextAbstractions<TDbContext>()
            where TDbContext : class, IApplicationDbContext
        {
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<TDbContext>());

            Type dbContextType = typeof(TDbContext);
            Type baseAbstractionType = typeof(IApplicationDbContext);

            Type[] dbContextAbstractionTypes = baseAbstractionType
                .Assembly
                .GetTypes()
                .Where(type =>
                    type.IsInterface &&
                    type != baseAbstractionType &&
                    baseAbstractionType.IsAssignableFrom(type))
                .ToArray();

            foreach (Type abstractionType in dbContextAbstractionTypes)
            {
                if (!abstractionType.IsAssignableFrom(dbContextType))
                {
                    throw new InvalidOperationException(
                        $"{dbContextType.Name} must implement {abstractionType.Name} because it derives from {baseAbstractionType.Name}.");
                }

                services.AddScoped(abstractionType, provider =>
                    provider.GetRequiredService<TDbContext>());
            }

            return services;
        }
    }
}
