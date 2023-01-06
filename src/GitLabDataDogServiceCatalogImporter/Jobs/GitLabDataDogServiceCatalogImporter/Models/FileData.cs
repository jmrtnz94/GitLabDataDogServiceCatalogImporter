namespace DataDogServiceCatalog.Jobs.GitLabDataDogServiceCatalogImporter.Models;

public record FileData(long ProjectId, string DefaultBranch, string FilePath);