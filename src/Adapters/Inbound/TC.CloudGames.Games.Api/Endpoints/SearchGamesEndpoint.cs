namespace TC.CloudGames.Games.Api.Endpoints;

/// <summary>
/// Endpoint for searching games using Elasticsearch.
/// Provides full-text search capabilities with pagination.
/// </summary>
public class SearchGamesEndpoint : Endpoint<SearchRequest>
{
    private readonly IGameElasticsearchService _search;
    private readonly ILogger<SearchGamesEndpoint> _logger;

    public SearchGamesEndpoint(IGameElasticsearchService search, ILogger<SearchGamesEndpoint> logger)
    {
        _search = search ?? throw new ArgumentNullException(nameof(search));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override void Configure()
    {
        Get("game/search");
        AllowAnonymous(); // Allow public search access

        Summary(s =>
        {
            s.Summary = "Search games using full-text search";
            s.Description = "This endpoint provides full-text search capabilities across game names, descriptions, genres, developers, and tags with fuzzy matching support.";
            s.Responses[200] = "Search results returned successfully";
            s.Responses[400] = "Bad request - invalid search parameters";
            s.Responses[500] = "Internal server error";
        });
    }

    public override async Task HandleAsync(SearchRequest req, CancellationToken ct)
    {
        try
        {
            // Validate search parameters
            if (req.Size is < 1 or > 100)
            {
                AddError("Size must be between 1 and 100", "Size.Invalid");
                await Send.ErrorsAsync(cancellation: ct);
                return;
            }

            _logger.LogInformation("Searching games with query: '{Query}' (size: {Size})", req.Query, req.Size);

            // Perform search
            var result = await _search.SearchAsync(req.Query, req.Size, ct);

            _logger.LogInformation("Search completed: {HitCount} hits found (total: {Total})",
                result.Hits.Count, result.Total);

            // Return structured response
            var response = new
            {
                Query = req.Query,
                Size = req.Size,
                Total = result.Total,
                Count = result.Hits.Count,
                Results = result.Hits
            };

            await Send.OkAsync(response, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching games with query '{Query}': {ErrorMessage}", req.Query, ex.Message);
            await Send.ErrorsAsync((int)System.Net.HttpStatusCode.InternalServerError, ct);
        }
    }
}

/// <summary>
/// Search request parameters for game search.
/// </summary>
/// <param name="Query">Search query text (can be empty for match-all)</param>
/// <param name="Size">Number of results to return (1-100, default: 20)</param>
public record SearchRequest(string Query = "", int Size = 20)
{
    /// <summary>
    /// Validates the search request parameters.
    /// </summary>
    public bool IsValid => Size is >= 1 and <= 100;
}
