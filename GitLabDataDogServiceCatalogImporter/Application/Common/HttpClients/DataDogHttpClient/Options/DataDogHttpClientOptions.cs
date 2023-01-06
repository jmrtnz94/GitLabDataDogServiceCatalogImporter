namespace DataDogServiceCatalog.Application.Common.HttpClients.DataDogHttpClient.Options;

public class DataDogHttpClientOptions
{
    public const string Name = "DataDogHttpClient";
    
    public string BaseAddress { get; set; } = string.Empty;
    public string DDApiKey { get; set; } = string.Empty;
    public string DDApplicationKey { get; set; } = string.Empty;
}