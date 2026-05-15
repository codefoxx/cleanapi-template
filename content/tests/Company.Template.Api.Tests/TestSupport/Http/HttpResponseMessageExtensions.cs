namespace Company.Template.Api.Tests.TestSupport.Http;

internal static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    extension(HttpResponseMessage response)
    {
        public async Task<TContract> ReadJsonAsync<TContract>(CancellationToken cancellationToken = default)
            where TContract : class
        {
            TContract? contract = await response.Content.ReadFromJsonAsync<TContract>(
                JsonOptions,
                cancellationToken);

            contract.ShouldNotBeNull();

            return contract;
        }

        public async Task<JsonDocument> ReadJsonDocumentAsync(CancellationToken cancellationToken = default)
        {
            Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
    }
}
