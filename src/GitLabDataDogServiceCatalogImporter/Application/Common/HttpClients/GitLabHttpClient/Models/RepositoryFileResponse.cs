namespace DataDogServiceCatalog.Application.Common.HttpClients.GitLabHttpClient.Models;

public record RepositoryFileResponse(string FileName, string Encoding, string Content);