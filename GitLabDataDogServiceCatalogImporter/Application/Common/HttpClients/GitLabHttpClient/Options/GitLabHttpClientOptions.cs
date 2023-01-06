namespace DataDogServiceCatalog.Application.Common.HttpClients.GitLabHttpClient.Options;

public class GitLabHttpClientOptions
{
    public const string Name = "GitLabHttpClient";

    public string BaseAddress { get; set; } = string.Empty;
    public string PrivateToken { get; set; } = string.Empty;
}