using TC.CloudGames.Games.Infrastructure.Elasticsearch;

namespace TC.CloudGames.Games.Api.Endpoints;

/// <summary>
/// Endpoint for retrieving popular games aggregation data by genre.
/// Provides analytics about game distribution across different genres.
/// </summary>
public class PopularGamesEndpoint : EndpointWithoutRequest
{
    private readonly IGameSearchService _search;

    public PopularGamesEndpoint(IGameSearchService search)
    {
        _search = search;
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

            Logger.LogInformation("📊 Retrieving popular games aggregation (size: {Size})", size);

            // Get aggregation data
            var result = await _search.GetPopularGamesAggregationAsync(size, ct);
            var genres = result.ToList();

            if (!genres.Any())
            {
                Logger.LogInformation("📭 No popular games data available");
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { Message = "No popular games data available" }, ct);
                return;
            }

            Logger.LogInformation("✅ Retrieved {GenreCount} popular genres", genres.Count);

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

            await HttpContext.Response.WriteAsJsonAsync(response, ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error retrieving popular games: {ErrorMessage}", ex.Message);
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { Error = "Internal server error" }, ct);
        }
    }
}