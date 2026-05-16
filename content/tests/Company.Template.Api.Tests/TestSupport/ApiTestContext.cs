namespace Company.Template.Api.Tests.TestSupport;

internal sealed class ApiTestContext : IAsyncDisposable, IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApiDatabaseTestFactory _factory;

    public ApiTestContext(
        TestDatabase database,
        ApiDatabaseTestFactory factory,
        HttpClient httpClient)
    {
        _database = database;
        _factory = factory;
        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; }

    public void Dispose()
    {
        HttpClient.Dispose();
        _factory.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        Dispose();
        await _database.DisposeAsync();
    }
}
