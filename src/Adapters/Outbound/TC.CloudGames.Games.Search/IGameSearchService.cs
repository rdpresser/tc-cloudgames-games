using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using TC.CloudGames.Games.Infrastructure.Projections;

namespace TC.CloudGames.Games.Search;

public interface IGameSearchService
{
    Task EnsureIndexAsync(CancellationToken ct = default);
    Task IndexAsync(GameProjection projection, CancellationToken ct = default);
    Task UpdateAsync(Guid id, object patch, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
    Task BulkIndexAsync(IEnumerable<GameProjection> games, CancellationToken ct = default);
    Task<SimpleSearchResult<GameProjection>> SearchAsync(string query, int size = 20, CancellationToken ct = default);

    Task<AggregateDictionary> GetPopularGamesAggregationAsync(int size = 10, CancellationToken ct = default);
}

public record SimpleSearchResult<T>(IReadOnlyCollection<T> Hits, long Total);
