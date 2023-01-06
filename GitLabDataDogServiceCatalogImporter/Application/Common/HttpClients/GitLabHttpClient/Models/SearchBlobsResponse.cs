namespace DataDogServiceCatalog.Application.Common.HttpClients.GitLabHttpClient.Models;

public record SearchBlobsResponse(long? Id, string Path, string Filename, long ProjectId);