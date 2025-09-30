using TC.CloudGames.Games.Search;
using TC.CloudGames.Games.Infrastructure.Projections;
using Marten;

namespace TC.CloudGames.Games.Api.Endpoints;

/// <summary>
/// Endpoint for reindexing games from the database to Elasticsearch.
/// This operation synchronizes the Elasticsearch index with the current database state.
/// </summary>
public class ReindexGamesEndpoint : EndpointWithoutRequest
{
    private readonly IGameSearchService _search;
    private readonly IDocumentStore _store;

    public ReindexGamesEndpoint(IGameSearchService search, IDocumentStore store)
    {
        _search = search;
        _store = store;
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
            using var session = _store.QuerySession();
            var games = await session.Query<GameProjection>()
                .Where(g => g.IsActive)
                .ToListAsync(ct);

            Logger.LogInformation("🔄 Starting reindex operation for {GameCount} games...", games.Count);

            if (!games.Any())
            {
                Logger.LogInformation("📭 No games found to reindex");
                await HttpContext.Response.WriteAsJsonAsync(new 
                { 
                    Message = "No games found to reindex", 
                    Count = 0 
                }, ct);
                return;
            }

            // Bulk index the games from database
            // In Elasticsearch Cloud, the index will be created automatically on first document
            await _search.BulkIndexAsync(games, ct);

            Logger.LogInformation("✅ Games reindexed successfully");

            await HttpContext.Response.WriteAsJsonAsync(new 
            { 
                Message = "Games reindexed successfully", 
                Count = games.Count,
                IndexName = "search-xn8c" 
            }, ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error reindexing games data: {ErrorMessage}", ex.Message);
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new 
            { 
                Error = "Failed to reindex games data",
                Message = ex.Message 
            }, ct);
        }
    }
}