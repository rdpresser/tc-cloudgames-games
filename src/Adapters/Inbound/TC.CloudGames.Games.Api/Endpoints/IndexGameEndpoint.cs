using TC.CloudGames.Games.Search;
using TC.CloudGames.Games.Infrastructure.Projections;

namespace TC.CloudGames.Games.Api.Endpoints;

/// <summary>
/// Endpoint for manually indexing a single game to Elasticsearch.
/// Typically used for testing or manual synchronization.
/// </summary>
public class IndexGameEndpoint : Endpoint<GameProjection>
{
    private readonly IGameSearchService _search;

    public IndexGameEndpoint(IGameSearchService search)
    {
        _search = search;
    }

    public override void Configure()
    {
        Post("game/index");
        Roles(AppConstants.AdminRole);

        Summary(s =>
        {
            s.Summary = "Index a single game to Elasticsearch";
            s.Description = "This endpoint allows for manual indexing of a single game document to the search index. Typically used for testing or troubleshooting purposes.";
            s.Responses[200] = "Game indexed successfully";
            s.Responses[400] = "Bad request - invalid game data";
            s.Responses[403] = "Access denied - admin role required";
            s.Responses[401] = "Unauthorized - valid token required";
            s.Responses[500] = "Internal server error";
        });
    }

    public override async Task HandleAsync(GameProjection req, CancellationToken ct)
    {
        try
        {
            // Validate required fields
            if (req.Id == Guid.Empty || string.IsNullOrWhiteSpace(req.Name))
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { Error = "Game ID and Name are required" }, ct);
                return;
            }

            Logger.LogInformation("📝 Indexing game: {GameName} (ID: {GameId})", req.Name, req.Id);

            // Index the game
            await _search.IndexAsync(req, ct);

            Logger.LogInformation("✅ Game indexed successfully: {GameName}", req.Name);

            // Return success response
            var response = new
            {
                Message = "Game indexed successfully",
                GameId = req.Id,
                GameName = req.Name,
                IndexName = "search-xn8c"
            };

            await HttpContext.Response.WriteAsJsonAsync(response, ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error indexing game {GameName} (ID: {GameId}): {ErrorMessage}", 
                req.Name, req.Id, ex.Message);
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { Error = "Internal server error" }, ct);
        }
    }
}