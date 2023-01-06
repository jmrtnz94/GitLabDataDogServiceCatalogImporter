using System.Text;
using System.Web;
using DataDogServiceCatalog.Application.Common.Extensions.HttpResponseMessageExtensions;
using DataDogServiceCatalog.Application.Common.HttpClients.Common;
using DataDogServiceCatalog.Application.Common.HttpClients.GitLabHttpClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataDogServiceCatalog.Application.Common.HttpClients.GitLabHttpClient;

public interface IGitLabHttpClient
{
    Task<IEnumerable<SearchBlobsResponse>> SearchBlobs(long groupId, string search, CancellationToken cancellationToken);
    Task<ProjectResponse?> GetProject(long projectId, CancellationToken cancellationToken);
    Task<RepositoryFileResponse?> GetRepositoryFile(long projectId, string filePath, string @ref, CancellationToken cancellationToken);
    Task<string?> GetRepositoryFileRaw(long projectId, string filePath, string @ref, CancellationToken cancellationToken);
}

public class GitLabHttpClient : BaseClient, IGitLabHttpClient
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        Formatting = Formatting.Indented
    };
    
    public GitLabHttpClient(HttpClient httpClient) : base(httpClient, JsonSerializerSettings)
    {
        
    }

    public async Task<IEnumerable<SearchBlobsResponse>> SearchBlobs(long groupId, string search, CancellationToken cancellationToken)
    {
        var searchBlobsResponseList = new List<SearchBlobsResponse>();
        
        // set default query params
        var queryParameters = new StringBuilder("?scope=blobs&order_by=created_at&sort=desc&per_page=100");

        if (!string.IsNullOrEmpty(search))
            queryParameters.Append($"&search=filename:{search}");

        var requestUrl = $"groups/{groupId}/search{queryParameters}";
        while (true)
        {
            var response = await SendRequest(HttpMethod.Get, requestUrl, cancellationToken);

            response.EnsureSuccessStatusCode();
            
            searchBlobsResponseList.AddRange(await Deserialize<IEnumerable<SearchBlobsResponse>>(response, cancellationToken) ?? Array.Empty<SearchBlobsResponse>());
            
            var headerWebLinks = response.ParseLinksHeader();

            if (!headerWebLinks.TryGetValue("next", out var nextLink))
                break;

            requestUrl = nextLink;
        }

        return  searchBlobsResponseList;
    }

    public async Task<ProjectResponse?> GetProject(long projectId, CancellationToken cancellationToken)
    {
        var response = await SendRequest(HttpMethod.Get, $"projects/{projectId}", cancellationToken);

        response.EnsureSuccessStatusCode();

        return await Deserialize<ProjectResponse>(response, cancellationToken);
    }

    public async Task<RepositoryFileResponse?> GetRepositoryFile(long projectId, string filePath, string @ref, CancellationToken cancellationToken)
    {
        var response = await SendRequest(HttpMethod.Get,
            $"projects/{projectId}/repository/files/{HttpUtility.UrlEncode(filePath)}?ref={@ref}", cancellationToken);

        response.EnsureSuccessStatusCode();

        return await Deserialize<RepositoryFileResponse>(response, cancellationToken);
    }

    public async Task<string?> GetRepositoryFileRaw(long projectId, string filePath, string @ref, CancellationToken cancellationToken)
    {
        var response = await SendRequest(HttpMethod.Get,
            $"projects/{projectId}/repository/files/{HttpUtility.UrlEncode(filePath)}/raw?ref={@ref}", cancellationToken);

        response.EnsureSuccessStatusCode();

        return await GetContent(response, cancellationToken);
    }
}