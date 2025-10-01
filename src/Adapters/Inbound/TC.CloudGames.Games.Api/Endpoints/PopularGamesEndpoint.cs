namespace TC.CloudGames.Games.Api.Endpoints;

/// <summary>
/// Endpoint for retrieving popular games aggregation data by genre.
/// Provides analytics about game distribution across different genres.
/// </summary>
public class PopularGamesEndpoint : EndpointWithoutRequest
{
    private readonly IGameElasticsearchService _search;
    private readonly ILogger<PopularGamesEndpoint> _logger;

    public PopularGamesEndpoint(IGameElasticsearchService search, ILogger<PopularGamesEndpoint> logger)
    {
        _search = search ?? throw new ArgumentNullException(nameof(search));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override void Configure()
    {
        Get("game/popular");
        AllowAnonymous(); // Allow public access to popular games data

        Summary(s =>
        {
            s.Summary = "Get popular games aggregation by genre";
            s.Description = "This endpoint returns aggregated data showing the most popular game genres with their respective game counts.";
            s.Responses[200] = "Popular games data returned successfully";
            s.Responses[404] = "No popular games data available";
            s.Responses[500] = "Internal server error";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            const int size = 10; // Fixed size for popular games

            _logger.LogInformation("Retrieving popular games aggregation (size: {Size})", size);

            // Get aggregation data
            var result = await _search.GetPopularGamesAggregationAsync(size, ct);
            var genres = result.ToList();

            if (!genres.Any())
            {
                _logger.LogInformation("No popular games data available");
                AddError("No popular games data available");
                await Send.ErrorsAsync((int)HttpStatusCode.NotFound, ct);
                return;
            }

            _logger.LogInformation("Retrieved {GenreCount} popular genres", genres.Count);

            // Return structured response
            var response = new
            {
                TotalGenres = genres.Count,
                TotalGames = genres.Sum(g => g.Count),
                Genres = genres.Select(g => new
                {
                    Genre = g.Genre,
                    GameCount = g.Count,
                    Percentage = genres.Sum(x => x.Count) > 0
                        ? Math.Round((double)g.Count / genres.Sum(x => x.Count) * 100, 2)
                        : 0
                }).ToList(),
                Timestamp = DateTimeOffset.UtcNow
            };

            await Send.OkAsync(response, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular games: {ErrorMessage}", ex.Message);
            await Send.ErrorsAsync((int)HttpStatusCode.InternalServerError, cancellation: ct);
        }
    }
}