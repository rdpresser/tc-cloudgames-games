using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net; // Add this for NetworkCredential

namespace TC.CloudGames.Games.Search;

public static class ElasticClientFactory
{
    public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var opts = configuration.GetSection("Elasticsearch").Get<ElasticSearchOptions>() ?? new ElasticSearchOptions();

        var uri = new Uri(opts.Url);
        var settings = new ElasticsearchClientSettings(uri);

        if (!string.IsNullOrWhiteSpace(opts.Username) && !string.IsNullOrWhiteSpace(opts.Password))
        {
            // Use Credentials property instead of BasicAuthentication
            settings = settings
                .Authentication(new BasicAuthentication(opts.Username!, opts.Password!));
        }

        // opcional: default index
        settings = settings.DefaultIndex(opts.IndexName);

        var client = new ElasticsearchClient(settings);
        services.AddSingleton(client);
        services.AddSingleton(opts);

        // registra serviços de domínio
        services.AddScoped<IGameSearchService, ElasticGameSearchService>();
    }
}