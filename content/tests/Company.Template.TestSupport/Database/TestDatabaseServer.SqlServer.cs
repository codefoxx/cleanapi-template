using Docker.DotNet.Models;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace Company.Template.TestSupport.Database;

/// <summary>
///     Owns the SQL Server container shared by database-backed tests.
/// </summary>
/// <remarks>
///     SQL Server containers are expensive to start and memory-heavy. This fixture starts one
///     server container and lets tests create isolated logical databases inside that server.
/// </remarks>
public sealed class TestDatabaseServer : IAsyncLifetime
{
    private const string MasterDatabase = "master";
    private const long SqlServerContainerMemoryLimitBytes = 4L * 1024 * 1024 * 1024;
    private const string SqlServerMemoryLimitMb = "3072";

    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("yourStrong(!)Password")
        .WithEnvironment("MSSQL_MEMORY_LIMIT_MB", SqlServerMemoryLimitMb)
        .WithCreateParameterModifier(parameters =>
        {
            parameters.HostConfig ??= new HostConfig();

            parameters.HostConfig.Memory = SqlServerContainerMemoryLimitBytes;

            // Docker supports this and it prevents slow swap-heavy test runs.
            // Some Docker-compatible runtimes may handle swap limits differently.
            parameters.HostConfig.MemorySwap = SqlServerContainerMemoryLimitBytes;
        })
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

        await using SqlConnection connection = new(CreateMasterConnectionString());
        await connection.OpenAsync();

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE {QuoteIdentifier(databaseName)};";

        await command.ExecuteNonQueryAsync();
    }

    public async Task DropDatabaseAsync(string databaseName)
    {
        ValidateDatabaseName(databaseName);

        await using SqlConnection connection = new(CreateMasterConnectionString());
        await connection.OpenAsync();

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = $"""
                               IF DB_ID({QuoteLiteral(databaseName)}) IS NOT NULL
                               BEGIN
                                   ALTER DATABASE {QuoteIdentifier(databaseName)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                   DROP DATABASE {QuoteIdentifier(databaseName)};
                               END
                               """;

        await command.ExecuteNonQueryAsync();
    }

    public string CreateConnectionString(string databaseName)
    {
        ValidateDatabaseName(databaseName);

        SqlConnectionStringBuilder builder = new(_container.GetConnectionString())
        {
            InitialCatalog = databaseName,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }

    private string CreateMasterConnectionString()
    {
        SqlConnectionStringBuilder builder = new(_container.GetConnectionString())
        {
            InitialCatalog = MasterDatabase,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }

    private static void ValidateDatabaseName(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("Database name is required.", nameof(databaseName));
        }

        if (databaseName.Length > 128)
        {
            throw new ArgumentException("Database name cannot exceed 128 characters.", nameof(databaseName));
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
        return $"[{value.Replace("]", "]]", StringComparison.Ordinal)}]";
    }

    private static string QuoteLiteral(string value)
    {
        return $"N'{value.Replace("'", "''", StringComparison.Ordinal)}'";
    }
}
