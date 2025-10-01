using TC.CloudGames.Games.Application.Abstractions.Ports;
using TC.CloudGames.Games.Application.Abstractions.Projections;

namespace TC.CloudGames.Games.Api.Endpoints;

/// <summary>
/// Endpoint for manually indexing a single game to Elasticsearch.
/// Typically used for testing or manual synchronization.
/// </summary>
public class IndexGameEndpoint : Endpoint<GameProjection>
{
    private readonly ILogger<IndexGameEndpoint> _logger;
    private readonly IGameElasticsearchService _search;

    public IndexGameEndpoint(ILogger<IndexGameEndpoint> logger, IGameElasticsearchService search)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _search = search ?? throw new ArgumentNullException(nameof(search));
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
                AddError("Game ID and Name are required", "Id|Name.Required");
                await Send.ErrorsAsync(cancellation: ct).ConfigureAwait(false);
                return;
            }

            _logger.LogInformation("Indexing game: {GameName} (ID: {GameId})", req.Name, req.Id);

            // Index the game
            await _search.IndexAsync(req, ct);

            _logger.LogInformation("Game indexed successfully: {GameName}", req.Name);

            // Return success response
            var response = new
            {
                Message = "Game indexed successfully",
                GameId = req.Id,
                GameName = req.Name,
                IndexName = "search-xn8c"
            };

            await Send.OkAsync(response, cancellation: ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing game {GameName} (ID: {GameId}): {ErrorMessage}", req.Name, req.Id, ex.Message);
            await Send.ErrorsAsync((int)HttpStatusCode.InternalServerError, cancellation: ct).ConfigureAwait(false);
        }
    }
}