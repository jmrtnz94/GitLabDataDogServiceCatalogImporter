namespace DataDogServiceCatalog.Jobs.GitLabDataDogServiceCatalogImporter.Options;

public class GitLabDataDogCrawlerOptions
{
    public const string Name = "GitLabDataDogCrawler";

    public string Filename { get; set; } = string.Empty;
    public long GroupId { get; set; }
}