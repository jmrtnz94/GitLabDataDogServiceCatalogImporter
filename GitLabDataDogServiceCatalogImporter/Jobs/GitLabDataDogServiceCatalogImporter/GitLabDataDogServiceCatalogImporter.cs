using DataDogServiceCatalog.Application.Common.HttpClients.DataDogHttpClient;
using DataDogServiceCatalog.Application.Common.HttpClients.GitLabHttpClient;
using DataDogServiceCatalog.Jobs.GitLabDataDogServiceCatalogImporter.Models;
using DataDogServiceCatalog.Jobs.GitLabDataDogServiceCatalogImporter.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataDogServiceCatalog.Jobs.GitLabDataDogServiceCatalogImporter;

public class GitLabDataDogServiceCatalogImporter : BackgroundService
{
    private readonly ILogger<GitLabDataDogServiceCatalogImporter> _logger;
    private readonly GitLabDataDogCrawlerOptions _gitLabDataDogCrawlerOptions;
    private readonly IGitLabHttpClient _gitLabHttpClient;
    private readonly IDataDogHttpClient _dataDogHttpClient;

    public GitLabDataDogServiceCatalogImporter(ILogger<GitLabDataDogServiceCatalogImporter> logger, IOptions<GitLabDataDogCrawlerOptions> gitLabDataDogCrawlerOptions, IGitLabHttpClient gitLabHttpClient, IDataDogHttpClient dataDogHttpClient)
    {
        _logger = logger;
        _gitLabDataDogCrawlerOptions = gitLabDataDogCrawlerOptions.Value;
        _gitLabHttpClient = gitLabHttpClient;
        _dataDogHttpClient = dataDogHttpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var searchBlobsResponses = (
            await _gitLabHttpClient.SearchBlobs(_gitLabDataDogCrawlerOptions.GroupId, _gitLabDataDogCrawlerOptions.Filename, cancellationToken)).ToList();

        if (!searchBlobsResponses.Any())
        {
            _logger.LogInformation("Search did not return any results");
            return;
        }

        var fileDataList = new List<FileData>();
        foreach (var searchBlobsResponse in searchBlobsResponses)
        {
            var projectResponse = await _gitLabHttpClient.GetProject(searchBlobsResponse.ProjectId, cancellationToken);
            
            if (projectResponse is not null)
                fileDataList.Add(new FileData(searchBlobsResponse.ProjectId, projectResponse.DefaultBranch, searchBlobsResponse.Path));
        }

        foreach (var fileData in fileDataList)
        {
            // download
            var datadogFileRaw = await _gitLabHttpClient.GetRepositoryFileRaw(fileData.ProjectId,
                fileData.FilePath, fileData.DefaultBranch, cancellationToken);
            
            // upload
            if (datadogFileRaw is not null)
                await _dataDogHttpClient.UpsertServiceDefinition(datadogFileRaw, cancellationToken);
        }
    }
}