using DataDogServiceCatalog.Application.Common.HttpClients.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataDogServiceCatalog.Application.Common.HttpClients.DataDogHttpClient;

public interface IDataDogHttpClient
{
    Task UpsertServiceDefinition(string content, CancellationToken cancellationToken);
}

public class DataDogHttpClient : BaseClient, IDataDogHttpClient
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new KebabCaseNamingStrategy()
        },
        Formatting = Formatting.Indented
    };

    public DataDogHttpClient(HttpClient httpClient) : base(httpClient, JsonSerializerSettings)
    {
        
    }

    public async Task UpsertServiceDefinition(string content, CancellationToken cancellationToken)
    {
        var response = await SendRequest(HttpMethod.Post, "services/definitions", cancellationToken, body: content);
        
        response.EnsureSuccessStatusCode();
    }
}