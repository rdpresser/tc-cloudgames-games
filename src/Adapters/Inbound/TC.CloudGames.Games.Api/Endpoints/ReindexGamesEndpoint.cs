using TC.CloudGames.Games.Application.Abstractions.Projections;
using TC.CloudGames.Games.Infrastructure.Repositories;

namespace TC.CloudGames.Games.Api.Endpoints;

/// <summary>
/// Endpoint for reindexing games from the database to Elasticsearch.
/// This operation synchronizes the Elasticsearch index with the current database state.
/// </summary>
public class ReindexGamesEndpoint : EndpointWithoutRequest
{
    private readonly IGameElasticsearchService _search;
    private readonly IGameProjectionStore _store;
    private readonly ILogger<ReindexGamesEndpoint> _logger;

    public ReindexGamesEndpoint(IGameElasticsearchService search, IGameProjectionStore store, ILogger<ReindexGamesEndpoint> logger)
    {
        _search = search ?? throw new ArgumentNullException(nameof(search));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override void Configure()
    {
        Post("game/reindex");
        Roles(AppConstants.AdminRole);

        Summary(s =>
        {
            s.Summary = "Reindex games from database to Elasticsearch";
            s.Description = "This endpoint reindexes all games from the database to the Elasticsearch index for search functionality. In Elasticsearch Cloud, indices are created automatically on first document.";
            s.Responses[200] = "Games reindexed successfully";
            s.Responses[400] = "Bad request";
            s.Responses[403] = "Access denied";
            s.Responses[401] = "Unauthorized";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // Get all active games from database
            var games = await _store.Query<GameProjection>()
                .Where(g => g.IsActive)
                .ToListAsync(ct);

            _logger.LogInformation("Starting reindex operation for {GameCount} games...", games.Count);

            if (!games.Any())
            {
                _logger.LogInformation("No games found to reindex");

                await Send.OkAsync(new
                {
                    Message = "No games found to reindex",
                    Count = 0
                }, ct);
                return;
            }

            // Bulk index the games from database
            // In Elasticsearch Cloud, the index will be created automatically on first document
            await _search.BulkIndexAsync(games, ct);

            _logger.LogInformation("✅ Games reindexed successfully");

            await Send.OkAsync(new
            {
                Message = "Games reindexed successfully",
                Count = games.Count,
                IndexName = "search-xn8c"
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reindexing games data: {ErrorMessage}", ex.Message);
            await Send.ErrorsAsync((int)System.Net.HttpStatusCode.InternalServerError, ct);
        }
    }
}