using System.Text;
using Newtonsoft.Json;

namespace DataDogServiceCatalog.Application.Common.HttpClients.Common;

    /// <summary>
    /// <para>
    /// <see cref="BaseClient"/> is the base class for typed <see cref="HttpClient" /> classes, pre-configured for
    /// specific use cases. This configuration can include specific values such as the base server, HTTP headers,
    /// or policy settings, e.g. retries or timeouts. Typed clients are registered as transient with DI container.
    /// </para>
    /// <remarks>
    /// <para>
    /// A Typed Client is effectively a transient object, which means a new instance is created each time one is needed.
    /// It receives a new HttpClient instance each time it's constructed. However, the HttpMessageHandler objects in the
    /// pool are the objects that are reused by multiple HttpClient instances. This pattern mitigates against any socket
    /// exhaustion that may be created through DI.
    /// </para>
    /// <para>
    /// See <seealso href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests" />
    /// </para>
    /// </remarks>
    /// </summary>
    public abstract class BaseClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        protected BaseClient(HttpClient httpClient, JsonSerializerSettings jsonSerializerSettings)
        {
            _httpClient = httpClient;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        protected async Task<HttpResponseMessage> SendRequest(HttpMethod httpMethod, string requestUri, CancellationToken cancellationToken, IDictionary<string, string>? requestHeaders = null, string? body = null)
        {
            var request = new HttpRequestMessage(httpMethod, requestUri);

            if (body != null)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            if (requestHeaders is not null && requestHeaders.Any())
            {
                foreach (var requestHeader in requestHeaders)
                {
                    request.Headers.Add(requestHeader.Key, requestHeader.Value);
                }
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);

            return response;
        }
        
        protected async Task<TReturn?> Deserialize<TReturn>(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
        {
            var stringContent = await GetContent(responseMessage, cancellationToken);

            return JsonConvert.DeserializeObject<TReturn>(stringContent, _jsonSerializerSettings);
        }

        protected string Serialize(object? body)
        {
            return JsonConvert.SerializeObject(body, _jsonSerializerSettings);
        }
        
        protected static async Task<string> GetContent(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
        {
            return await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        }
    }
