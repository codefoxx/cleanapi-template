namespace Company.Template.Api.Tests.TestSupport;

internal static class TestDatabaseServerExtensions
{
    public static async Task<ApiTestContext> CreateApiTestContextAsync(
        this TestDatabaseServer server)
    {
        TestDatabase database = await TestDatabase.CreateAsync(server);
        ApiDatabaseTestFactory factory = new(database);
        HttpClient httpClient = factory.CreateClient();

        return new ApiTestContext(database, factory, httpClient);
    }
}
