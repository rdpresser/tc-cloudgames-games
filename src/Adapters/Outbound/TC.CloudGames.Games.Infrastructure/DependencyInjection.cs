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
            // Check feature flag for Elasticsearch
            var isElasticsearchEnabled = configuration.GetValue<bool>("FeatureFlags:ElasticsearchEnabled", true);

            Console.WriteLine($"🔧 Elasticsearch Feature Flag: {(isElasticsearchEnabled ? "ENABLED" : "DISABLED")}");

            if (isElasticsearchEnabled)
            {
                // Configure real Elasticsearch services
                services.Configure<ElasticSearchOptions>(configuration.GetSection("Elasticsearch"));
                services.AddSingleton<IElasticsearchClientProvider, ElasticsearchClientProvider>();
                services.AddSingleton(sp => sp.GetRequiredService<IOptions<ElasticSearchOptions>>().Value);
                services.AddScoped<IGameElasticsearchService, GameElasticsearchService>();

                Console.WriteLine("✅ Elasticsearch is ENABLED - Real services registered");
            }
            else
            {
                // Configure fake Elasticsearch services
                services.AddSingleton<IElasticsearchClientProvider, FakeElasticsearchClientProvider>();
                services.AddScoped<IGameElasticsearchService, FakeGameElasticsearchService>();

                Console.WriteLine("⚠️ Elasticsearch is DISABLED - Fake services registered that will log operations");
            }

            return services;
        }
    }
}
