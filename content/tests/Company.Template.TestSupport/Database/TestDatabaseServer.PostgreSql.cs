using Npgsql;
using Testcontainers.PostgreSql;

namespace Company.Template.TestSupport.Database;

/// <summary>
///     Owns the PostgreSQL container shared by database-backed tests.
/// </summary>
/// <remarks>
///     The container represents the expensive database server. Individual tests should create
///     isolated logical databases through this fixture instead of starting additional containers.
/// </remarks>
public sealed class TestDatabaseServer : IAsyncLifetime
{
    private const string ManagementDatabase = "postgres";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
                                                     .WithImage("postgres:18")
                                                     .WithDatabase(ManagementDatabase)
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

    public async Task CreateDatabaseAsync(string databaseName)
    {
        ValidateDatabaseName(databaseName);

        await using NpgsqlConnection connection = new(CreateManagementConnectionString());
        await connection.OpenAsync();

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = $"""
                               CREATE DATABASE {QuoteIdentifier(databaseName)};
                               """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task DropDatabaseAsync(string databaseName)
    {
        ValidateDatabaseName(databaseName);

        await using NpgsqlConnection connection = new(CreateManagementConnectionString());
        await connection.OpenAsync();

        await TerminateConnectionsAsync(connection, databaseName);

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = $"""
                               DROP DATABASE IF EXISTS {QuoteIdentifier(databaseName)};
                               """;

        await command.ExecuteNonQueryAsync();
    }

    public string CreateConnectionString(string databaseName)
    {
        ValidateDatabaseName(databaseName);

        NpgsqlConnectionStringBuilder builder = new(_container.GetConnectionString())
        {
            Database = databaseName
        };

        return builder.ConnectionString;
    }

    private string CreateManagementConnectionString()
    {
        NpgsqlConnectionStringBuilder builder = new(_container.GetConnectionString())
        {
            Database = ManagementDatabase
        };

        return builder.ConnectionString;
    }

    private static async Task TerminateConnectionsAsync(
        NpgsqlConnection connection,
        string databaseName)
    {
        await using NpgsqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT pg_terminate_backend(pid)
                              FROM pg_stat_activity
                              WHERE datname = @databaseName
                                AND pid <> pg_backend_pid();
                              """;

        command.Parameters.AddWithValue("databaseName", databaseName);

        await command.ExecuteNonQueryAsync();
    }

    private static void ValidateDatabaseName(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("Database name is required.", nameof(databaseName));
        }

        if (databaseName.Length > 63)
        {
            throw new ArgumentException("PostgreSQL database name cannot exceed 63 characters.", nameof(databaseName));
        }

        if (databaseName.Any(character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character != '_'))
        {
            throw new ArgumentException(
                "Database name may only contain ASCII letters, digits, and underscores.",
                nameof(databaseName));
        }
    }

    private static string QuoteIdentifier(string value)
    {
        return $"""
                "{value.Replace("\"", "\"\"", StringComparison.Ordinal)}"
                """;
    }
}
