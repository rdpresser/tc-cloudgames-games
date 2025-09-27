using TC.CloudGames.Games.Search;

namespace TC.CloudGames.Games.Api.Endpoints;

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
            s.Description = "This endpoint reindexes all games from the database to the Elasticsearch index for search functionality.";
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
            // Ensure index exists
            await _search.EnsureIndexAsync(ct);

            // Get all games from database
            using var session = _store.QuerySession();
            var games = await session.Query<GameProjection>().ToListAsync(ct);
            
            Console.WriteLine($"🔄 Reindexing {games.Count} games from database...");
            
            // Bulk index the games from database
            await _search.BulkIndexAsync(games, ct);
            Console.WriteLine($"✅ Games reindexed successfully");

            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Games reindexed successfully", Count = games.Count }, ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error reindexing games data");
            await HttpContext.Response.WriteAsJsonAsync(new { Error = "Failed to reindex games data" }, ct);
        }
    }
}