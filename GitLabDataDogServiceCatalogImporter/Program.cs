using System.Net.Http.Headers;
using DataDogServiceCatalog.Application.Common.HttpClients.DataDogHttpClient;
using DataDogServiceCatalog.Application.Common.HttpClients.DataDogHttpClient.Options;
using DataDogServiceCatalog.Application.Common.HttpClients.GitLabHttpClient;
using DataDogServiceCatalog.Application.Common.HttpClients.GitLabHttpClient.Options;
using DataDogServiceCatalog.Jobs.GitLabDataDogServiceCatalogImporter;
using DataDogServiceCatalog.Jobs.GitLabDataDogServiceCatalogImporter.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Json;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient<IGitLabHttpClient, GitLabHttpClient>(options =>
        {
            var httpOptions = new GitLabHttpClientOptions();
            context.Configuration.GetSection(GitLabHttpClientOptions.Name).Bind(httpOptions);
            
            options.BaseAddress = new Uri(httpOptions.BaseAddress);
            options.DefaultRequestHeaders.Add("PRIVATE-TOKEN", httpOptions.PrivateToken);
            options.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
        
        services.AddHttpClient<IDataDogHttpClient, DataDogHttpClient>(options =>
        {
            var httpOptions = new DataDogHttpClientOptions();
            context.Configuration.GetSection(DataDogHttpClientOptions.Name).Bind(httpOptions);
            
            options.BaseAddress = new Uri(httpOptions.BaseAddress);
            options.DefaultRequestHeaders.Add("DD-API-KEY", httpOptions.DDApiKey);
            options.DefaultRequestHeaders.Add("DD-APPLICATION-KEY", httpOptions.DDApplicationKey);
            options.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.Configure<GitLabDataDogCrawlerOptions>(
            context.Configuration.GetSection(GitLabDataDogCrawlerOptions.Name));

        services.AddHostedService<GitLabDataDogServiceCatalogImporter>();
    })
    .UseSerilog((context, configure) =>
    {
        configure
            .ReadFrom.Configuration(context.Configuration);

        if (context.HostingEnvironment.IsDevelopment())
        {
            configure.WriteTo.Console();
        }
        else
        {
            configure.WriteTo.Console(new JsonFormatter(renderMessage: true));
        }
    })
    .RunConsoleAsync();