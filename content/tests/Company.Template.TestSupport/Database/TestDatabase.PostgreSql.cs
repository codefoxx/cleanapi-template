using Microsoft.EntityFrameworkCore;

namespace Company.Template.TestSupport.Database;

public sealed partial class TestDatabase
{
    static partial void ConfigureProvider(
        DbContextOptionsBuilder builder,
        string connectionString)
    {
        builder.UseNpgsql(connectionString);
    }
}