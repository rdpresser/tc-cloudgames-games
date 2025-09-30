using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TC.CloudGames.Games.Search;

/// <summary>
/// Factory for creating and configuring Elasticsearch clients.
/// Supports both Elasticsearch Cloud Serverless and local development.
/// </summary>
public static class ElasticClientFactory
{
    /// <summary>
    /// Adds Elasticsearch services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
    public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        // Load and validate configuration
        var options = configuration.GetSection("Elasticsearch").Get<ElasticSearchOptions>() ?? new ElasticSearchOptions();
        options.Validate();

        // Create and configure Elasticsearch client
        var client = CreateElasticsearchClient(options);
        
        // Register services
        services.AddSingleton(options);
        services.AddSingleton(client);
        services.AddScoped<IGameSearchService, ElasticGameSearchService>();
    }

    /// <summary>
    /// Creates an Elasticsearch client based on the provided options.
    /// </summary>
    /// <param name="options">Elasticsearch configuration options</param>
    /// <returns>Configured Elasticsearch client</returns>
    private static ElasticsearchClient CreateElasticsearchClient(ElasticSearchOptions options)
    {
        var connectionUrl = options.GetConnectionUrl();
        var uri = new Uri(connectionUrl);

        var settings = new ElasticsearchClientSettings(uri)
            .DefaultIndex(options.IndexName)
            .RequestTimeout(TimeSpan.FromSeconds(options.RequestTimeoutSeconds))
            .PingTimeout(TimeSpan.FromSeconds(10))
            .DeadTimeout(TimeSpan.FromMinutes(2))
            .MaxDeadTimeout(TimeSpan.FromMinutes(5));

        // Configure authentication based on environment
        if (options.IsElasticCloud && !string.IsNullOrWhiteSpace(options.ApiKey))
        {
            // Use API Key authentication for Elasticsearch Cloud (both regular and serverless)
            settings = settings.Authentication(new ApiKey(options.ApiKey!));
        }
        else if (options.IsLocal && !string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password))
        {
            // Use Basic authentication for local development
            settings = settings.Authentication(new BasicAuthentication(options.Username!, options.Password!));
        }

        // Enable detailed diagnostics in development
        if (options.IsLocal)
        {
            settings = settings
                .DisableDirectStreaming()
                .PrettyJson()
                .EnableDebugMode();
        }

        return new ElasticsearchClient(settings);
    }
}