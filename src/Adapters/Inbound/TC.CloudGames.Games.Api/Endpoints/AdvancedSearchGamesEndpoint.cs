using TC.CloudGames.Games.Infrastructure.Elasticsearch;

namespace TC.CloudGames.Games.Api.Endpoints;

/// <summary>
/// Endpoint for advanced game search with filtering capabilities.
/// Provides comprehensive search functionality with multiple filters and sorting options.
/// </summary>
public class AdvancedSearchGamesEndpoint : Endpoint<AdvancedSearchRequest>
{
    private readonly IGameSearchService _search;

    public AdvancedSearchGamesEndpoint(IGameSearchService search)
    {
        _search = search;
    }

    public override void Configure()
    {
        Post("game/search/advanced");
        AllowAnonymous(); // Allow public access to search

        Summary(s =>
        {
            s.Summary = "Advanced game search with filters";
            s.Description = "This endpoint provides advanced search capabilities with support for filtering by genre, platform, price range, rating, release date, and sorting options.";
            s.Responses[200] = "Search results returned successfully";
            s.Responses[400] = "Bad request - invalid search parameters";
            s.Responses[500] = "Internal server error";
        });
    }

    public override async Task HandleAsync(AdvancedSearchRequest req, CancellationToken ct)
    {
        try
        {
            // Validate request
            if (!req.IsValid)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { Error = "Invalid search parameters" }, ct);
                return;
            }

            Logger.LogInformation("🔍 Advanced search: Query='{Query}', Genres={Genres}, Platforms={Platforms}, Size={Size}",
                req.Query,
                req.Genres != null ? string.Join(",", req.Genres) : "none",
                req.Platforms != null ? string.Join(",", req.Platforms) : "none",
                req.Size);

            // Create search request
            var searchRequest = new GameSearchRequest
            {
                Query = req.Query ?? string.Empty,
                Genres = req.Genres,
                Platforms = req.Platforms,
                MinPrice = req.MinPrice,
                MaxPrice = req.MaxPrice,
                MinRating = req.MinRating,
                ReleaseDateFrom = req.ReleaseDateFrom,
                ReleaseDateTo = req.ReleaseDateTo,
                Size = req.Size,
                From = req.From,
                SortBy = req.SortBy,
                SortDirection = req.SortDirection
            };

            // Perform advanced search
            var result = await _search.SearchAdvancedAsync(searchRequest, ct);

            Logger.LogInformation("✅ Advanced search completed: {HitCount} hits found (total: {Total})",
                result.Hits.Count, result.Total);

            // Return structured response
            var response = new
            {
                Query = req.Query,
                Filters = new
                {
                    req.Genres,
                    req.Platforms,
                    PriceRange = req.MinPrice.HasValue || req.MaxPrice.HasValue
                        ? new { Min = req.MinPrice, Max = req.MaxPrice }
                        : null,
                    req.MinRating,
                    ReleaseDateRange = req.ReleaseDateFrom.HasValue || req.ReleaseDateTo.HasValue
                        ? new { From = req.ReleaseDateFrom, To = req.ReleaseDateTo }
                        : null
                },
                Pagination = new
                {
                    req.Size,
                    req.From,
                    Total = result.Total,
                    CurrentPage = (req.From / req.Size) + 1,
                    TotalPages = (int)Math.Ceiling((double)result.Total / req.Size)
                },
                Sorting = new
                {
                    req.SortBy,
                    req.SortDirection
                },
                Count = result.Hits.Count,
                Results = result.Hits
            };

            await HttpContext.Response.WriteAsJsonAsync(response, ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error in advanced search: {ErrorMessage}", ex.Message);
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { Error = "Internal server error" }, ct);
        }
    }
}

/// <summary>
/// Advanced search request with comprehensive filtering options.
/// </summary>
public record AdvancedSearchRequest
{
    /// <summary>
    /// Search query text (optional).
    /// </summary>
    public string? Query { get; init; }

    /// <summary>
    /// Filter by specific genres (optional).
    /// </summary>
    public IReadOnlyCollection<string>? Genres { get; init; }

    /// <summary>
    /// Filter by platforms (optional).
    /// </summary>
    public IReadOnlyCollection<string>? Platforms { get; init; }

    /// <summary>
    /// Minimum price filter (optional).
    /// </summary>
    public decimal? MinPrice { get; init; }

    /// <summary>
    /// Maximum price filter (optional).
    /// </summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>
    /// Minimum rating filter (optional).
    /// </summary>
    public decimal? MinRating { get; init; }

    /// <summary>
    /// Release date range - from date (optional).
    /// </summary>
    public DateOnly? ReleaseDateFrom { get; init; }

    /// <summary>
    /// Release date range - to date (optional).
    /// </summary>
    public DateOnly? ReleaseDateTo { get; init; }

    /// <summary>
    /// Number of results to return (1-100, default: 20).
    /// </summary>
    public int Size { get; init; } = 20;

    /// <summary>
    /// Starting offset for pagination (default: 0).
    /// </summary>
    public int From { get; init; } = 0;

    /// <summary>
    /// Sort field (optional).
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Sort direction ("asc" or "desc", default: "desc").
    /// </summary>
    public string SortDirection { get; init; } = "desc";

    /// <summary>
    /// Validates the search request parameters.
    /// </summary>
    public bool IsValid =>
        Size is >= 1 and <= 100 &&
        From >= 0 &&
        (MinPrice == null || MinPrice >= 0) &&
        (MaxPrice == null || MaxPrice >= 0) &&
        (MinPrice == null || MaxPrice == null || MinPrice <= MaxPrice) &&
        (MinRating == null || MinRating is >= 0 and <= 10) &&
        (ReleaseDateFrom == null || ReleaseDateTo == null || ReleaseDateFrom <= ReleaseDateTo) &&
        SortDirection is "asc" or "desc";
}