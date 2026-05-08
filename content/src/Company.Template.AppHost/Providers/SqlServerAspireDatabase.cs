using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Company.Template.AppHost.Providers;

internal static class AspireDatabase
{
    public static IResourceBuilder<IResourceWithConnectionString> Create(IDistributedApplicationBuilder builder)
    {
        return builder
            .AddSqlServer("sqlserver")
            .AddDatabase("DefaultConnection");
    }
}
