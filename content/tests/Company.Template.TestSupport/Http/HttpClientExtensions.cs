namespace Company.Template.TestSupport.Http;

public static class HttpClientExtensions
{
    extension(HttpClient client)
    {
        public Task<HttpResponseMessage> SendAsync(ApiEndpoint endpoint,
            CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = new(endpoint.Method, endpoint.ToUri());

            return client.SendAsync(request, cancellationToken);
        }

        public Task<HttpResponseMessage> SendJsonAsync<TBody>(ApiEndpoint endpoint,
            TBody body,
            CancellationToken cancellationToken = default)
            where TBody : notnull
        {
            HttpRequestMessage request = new(endpoint.Method, endpoint.ToUri())
            {
                Content = JsonContent.Create(body)
            };

            return client.SendAsync(request, cancellationToken);
        }
    }
}
