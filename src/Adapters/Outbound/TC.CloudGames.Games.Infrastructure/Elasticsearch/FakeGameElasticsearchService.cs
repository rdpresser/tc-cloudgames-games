using Microsoft.Extensions.Logging;
using TC.CloudGames.Games.Application.Abstractions.Ports;
using TC.CloudGames.Games.Application.Abstractions.Projections;

namespace TC.CloudGames.Games.Infrastructure.Elasticsearch;

/// <summary>
/// Fake implementation of IGameElasticsearchService for when Elasticsearch is disabled.
/// Logs operations instead of executing them and returns empty results.
/// </summary>
public sealed class FakeGameElasticsearchService : IGameElasticsearchService
{
    private readonly ILogger<FakeGameElasticsearchService> _logger;

    public FakeGameElasticsearchService(ILogger<FakeGameElasticsearchService> logger)
    {
        _logger = logger;
    }

    public Task IndexAsync(GameProjection projection, CancellationToken ct = default)
    {
        _logger.LogInformation("?? Elasticsearch DISABLED - Would index game: {GameName} (ID: {GameId})", 
         projection.Name, projection.Id);
        return Task.CompletedTask;
    }

    public Task BulkIndexAsync(IEnumerable<GameProjection> games, CancellationToken ct = default)
    {
        var gamesList = games.ToList();
        _logger.LogInformation("?? Elasticsearch DISABLED - Would bulk index {GameCount} games", gamesList.Count);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid id, object patch, CancellationToken ct = default)
    {
        _logger.LogInformation("?? Elasticsearch DISABLED - Would update game with ID: {GameId}", id);
  return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken ct = default)
  {
        _logger.LogInformation("?? Elasticsearch DISABLED - Would delete game with ID: {GameId}", id);
        return Task.CompletedTask;
    }

    public Task<SimpleSearchResult<GameProjection>> SearchAsync(string query, int size = 20, CancellationToken ct = default)
    {
        _logger.LogWarning("?? Elasticsearch DISABLED - Search for '{Query}' returning empty results", query);
        return Task.FromResult(new SimpleSearchResult<GameProjection>(Array.Empty<GameProjection>(), 0));
    }

    public Task<SimpleSearchResult<GameProjection>> SearchAdvancedAsync(GameSearchRequest searchRequest, CancellationToken ct = default)
    {
        _logger.LogWarning("?? Elasticsearch DISABLED - Advanced search for '{Query}' returning empty results", searchRequest.Query);
 return Task.FromResult(new SimpleSearchResult<GameProjection>(Array.Empty<GameProjection>(), 0));
    }

    public Task<IEnumerable<PopularGenreResult>> GetPopularGamesAggregationAsync(int size = 10, CancellationToken ct = default)
    {
        _logger.LogWarning("?? Elasticsearch DISABLED - Popular games aggregation returning empty results");
  return Task.FromResult(Enumerable.Empty<PopularGenreResult>());
    }
}