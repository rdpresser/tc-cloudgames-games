using TC.CloudGames.Games.Infrastructure.Projections;

namespace TC.CloudGames.Games.Search;

/// <summary>
/// Service interface for game search operations.
/// Optimized for Elasticsearch Cloud Serverless with automatic index management.
/// </summary>
public interface IGameSearchService
{
    /// <summary>
    /// Indexes a single game document.
    /// </summary>
    /// <param name="projection">Game projection to index</param>
    /// <param name="ct">Cancellation token</param>
    Task IndexAsync(GameProjection projection, CancellationToken ct = default);

    /// <summary>
    /// Updates a game document with partial data.
    /// </summary>
    /// <param name="id">Game ID</param>
    /// <param name="patch">Partial update data</param>
    /// <param name="ct">Cancellation token</param>
    Task UpdateAsync(Guid id, object patch, CancellationToken ct = default);

    /// <summary>
    /// Deletes a game document by ID.
    /// </summary>
    /// <param name="id">Game ID to delete</param>
    /// <param name="ct">Cancellation token</param>
    Task DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Indexes multiple games in a single bulk operation.
    /// </summary>
    /// <param name="games">Games to index</param>
    /// <param name="ct">Cancellation token</param>
    Task BulkIndexAsync(IEnumerable<GameProjection> games, CancellationToken ct = default);

    /// <summary>
    /// Searches games using full-text search.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="size">Number of results to return</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Search results</returns>
    Task<SimpleSearchResult<GameProjection>> SearchAsync(string query, int size = 20, CancellationToken ct = default);

    /// <summary>
    /// Gets aggregated data for popular games by genre.
    /// </summary>
    /// <param name="size">Number of results to return</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Aggregation results</returns>
    Task<IEnumerable<PopularGenreResult>> GetPopularGamesAggregationAsync(int size = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches games with advanced filtering options.
    /// </summary>
    /// <param name="searchRequest">Advanced search request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Search results</returns>
    Task<SimpleSearchResult<GameProjection>> SearchAdvancedAsync(GameSearchRequest searchRequest, CancellationToken ct = default);
}

/// <summary>
/// Represents search results with metadata.
/// </summary>
/// <typeparam name="T">Type of search result items</typeparam>
/// <param name="Hits">Collection of result items</param>
/// <param name="Total">Total number of matching documents</param>
public record SimpleSearchResult<T>(IReadOnlyCollection<T> Hits, long Total);

/// <summary>
/// Represents a popular genre aggregation result.
/// </summary>
/// <param name="Genre">Genre name</param>
/// <param name="Count">Number of games in this genre</param>
public record PopularGenreResult(string Genre, long Count);

/// <summary>
/// Advanced search request with filtering options.
/// </summary>
public record GameSearchRequest
{
    /// <summary>
    /// Search query text.
    /// </summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Filter by specific genres.
    /// </summary>
    public IReadOnlyCollection<string>? Genres { get; init; }

    /// <summary>
    /// Filter by platforms.
    /// </summary>
    public IReadOnlyCollection<string>? Platforms { get; init; }

    /// <summary>
    /// Minimum price filter.
    /// </summary>
    public decimal? MinPrice { get; init; }

    /// <summary>
    /// Maximum price filter.
    /// </summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>
    /// Minimum rating filter.
    /// </summary>
    public decimal? MinRating { get; init; }

    /// <summary>
    /// Release date range - from date.
    /// </summary>
    public DateOnly? ReleaseDateFrom { get; init; }

    /// <summary>
    /// Release date range - to date.
    /// </summary>
    public DateOnly? ReleaseDateTo { get; init; }

    /// <summary>
    /// Number of results to return.
    /// </summary>
    public int Size { get; init; } = 20;

    /// <summary>
    /// Starting offset for pagination.
    /// </summary>
    public int From { get; init; } = 0;

    /// <summary>
    /// Sort field (e.g., "releaseDate", "priceAmount", "ratingAverage").
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Sort direction ("asc" or "desc").
    /// </summary>
    public string SortDirection { get; init; } = "desc";
}
