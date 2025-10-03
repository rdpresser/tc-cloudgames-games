using Microsoft.Extensions.Configuration;
using TC.CloudGames.Games.Infrastructure.Elasticsearch;
using TC.CloudGames.SharedKernel.Infrastructure.Snapshots.Users;

namespace TC.CloudGames.Games.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IUserGameLibraryRepository, UserGameLibraryRepository>();
            services.AddSingleton<ICacheProvider, CacheProvider>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();
            services.AddSingleton<ITokenProvider, TokenProvider>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IUserSnapshotStore, UserSnapshotStore>();
            services.AddScoped<IGameProjectionStore, GameProjectionStore>();

            return services;
        }

        public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Elasticsearch options
            services.Configure<ElasticSearchOptions>(configuration.GetSection("Elasticsearch"));

            // Register Elasticsearch client provider and client
            services.AddSingleton<IElasticsearchClientProvider, ElasticsearchClientProvider>();
            ////services.AddSingleton(sp => sp.GetRequiredService<IElasticsearchClientProvider>().Client);

            // Register ElasticSearchOptions for direct injection
            ////services.AddSingleton(sp => sp.GetRequiredService<IOptions<ElasticSearchOptions>>().Value);

            // Register search services
            services.AddScoped<IGameElasticsearchService, GameElasticsearchService>();

            return services;
        }
    }
}
