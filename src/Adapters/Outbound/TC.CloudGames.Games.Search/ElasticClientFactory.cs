using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TC.CloudGames.Games.Search;

public static class ElasticClientFactory
{
    public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var opts = configuration.GetSection("Elasticsearch").Get<ElasticSearchOptions>() ?? new ElasticSearchOptions();

        var connectionUrl = opts.GetConnectionUrl();
        if (string.IsNullOrWhiteSpace(connectionUrl))
            throw new InvalidOperationException("Elasticsearch URL não configurada. Verifique seu .env ou appsettings.json.");

        var uri = new Uri(connectionUrl);

        var settings = new ElasticsearchClientSettings(uri)
            .DefaultIndex(opts.IndexName)
            .RequestTimeout(TimeSpan.FromSeconds(30))
            .PingTimeout(TimeSpan.FromSeconds(10))
            .DeadTimeout(TimeSpan.FromMinutes(2))
            .MaxDeadTimeout(TimeSpan.FromMinutes(5))
            .ThrowExceptions();

        // Only add authentication if credentials are provided
        if (!string.IsNullOrWhiteSpace(opts.Username) && !string.IsNullOrWhiteSpace(opts.Password))
        {
            settings = settings.Authentication(new BasicAuthentication(opts.Username!, opts.Password!));
        }

        var client = new ElasticsearchClient(settings);
        services.AddSingleton(client);
        services.AddSingleton(opts);

        services.AddScoped<IGameSearchService, ElasticGameSearchService>();
        services.AddScoped<GamesIndexInitializer>();
    }
}