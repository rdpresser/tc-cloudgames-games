using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Microsoft.Extensions.Logging;
using TC.CloudGames.Games.Infrastructure.Projections;

namespace TC.CloudGames.Games.Search;

public class ElasticGameSearchService : IGameSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticSearchOptions _options;
    private readonly ILogger<ElasticGameSearchService> _logger; 

    public ElasticGameSearchService(
        ElasticsearchClient client,
        ElasticSearchOptions options,
        ILogger<ElasticGameSearchService> logger)
    {
        _client = client;
        _options = options;
        _logger = logger;
    }

    public async Task BulkIndexAsync(IEnumerable<GameProjection> games, CancellationToken ct = default)
    {
        var gamesList = games.ToList();
        _logger.LogInformation("📦 Bulk indexing {GameCount} games to index: {IndexName}",gamesList.Count, _options.IndexName);

        // Ensure index exists before bulk indexing
        await EnsureIndexAsync(ct);
        
        var operations = new List<IBulkOperation>();
        foreach (var g in gamesList)
        {
            _logger.LogDebug("📝 Adding game: {GameName} (Genre: {Genre})", g.Name, g.Genre);
            operations.Add(new BulkIndexOperation<GameProjection>(g) { Id = g.Id });
        }

        var bulkReq = new BulkRequest
        {
            Index = _options.IndexName,
            Operations = operations
        };

        var response = await _client.BulkAsync(bulkReq, ct);
        _logger.LogInformation("📊 Bulk response valid: {IsValidResponse}", response.IsValidResponse);

        if (!response.IsValidResponse)
        {
            _logger.LogError("❌ Bulk indexing failed: {DebugInformation}", response.DebugInformation);
        }
        else
        {
            _logger.LogInformation("✅ Bulk indexing completed successfully for {GameCount} games", gamesList.Count);
        }
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        _logger.LogInformation("🗑️ Deleting game with ID: {GameId} from index: {IndexName}", id, _options.IndexName);

        var request = new DeleteRequest(_options.IndexName, id);
        var response = await _client.DeleteAsync(request, ct);

        if (response.IsValidResponse)
        {
            _logger.LogInformation("✅ Game {GameId} deleted successfully", id);
        }
        else
        {
            _logger.LogWarning("⚠️ Game {GameId} deletion response: {DebugInformation}", id, response.DebugInformation);
        }
    }

    public async Task EnsureIndexAsync(CancellationToken ct = default)
    {
        var exists = await _client.Indices.ExistsAsync(_options.IndexName, ct);
        if (exists.Exists)
        {
            _logger.LogDebug("📊 Index {IndexName} already exists", _options.IndexName);
            return;
        }

        _logger.LogInformation("📝 Creating index: {IndexName}", _options.IndexName);

        await _client.Indices.CreateAsync(_options.IndexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("autocomplete_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filter("lowercase", "asciifolding")
                        )
                    )
                )
            )
            .Mappings(m => m
                .Properties<GameProjection>(p => p
                    .Text("name")
                    .Text("description")
                    .Keyword("genre")
                    .Keyword("platforms")
                    .IntegerNumber("playerCount")
                    .Date("releaseDate")
                    .Boolean("isActive")
                )
            )
        , ct);

        await _client.Indices.PutAliasAsync(_options.IndexName, "games", ct);

        _logger.LogInformation("✅ Index {IndexName} created successfully with alias 'games'", _options.IndexName);
    }

    public async Task<IEnumerable<object>> GetPopularGamesAggregationAsync(int size = 10, CancellationToken ct = default)
    {
        try
        {

            _logger.LogInformation("🔍 Searching popular games aggregation in index: {IndexName} with size: {Size}", _options.IndexName, size);

            // Verificar se o índice existe
            var indexExists = await _client.Indices.ExistsAsync(_options.IndexName, ct);

            _logger.LogDebug("📊 Index exists: {IndexExists}", indexExists.Exists);

            if (!indexExists.Exists)
            {
                _logger.LogInformation("📝 Creating index for aggregation search...");
                await EnsureIndexAsync(ct);
            }

            // Verificar se há documentos no índice
            _logger.LogDebug("📈 Checking documents in index: {IndexName}", _options.IndexName);

            var resp = await _client.SearchAsync<GameProjection>(s => s
                .Indices(_options.IndexName)
                .Size(0)
                .Query(q => q.MatchAll())
                .Aggregations(a => a
                    .Add("top_games", new TermsAggregation
                    {
                        Field = "genre",
                        Size = size,
                        MinDocCount = 1
                    })
                ), ct);

            _logger.LogDebug("🔍 Search response valid: {IsValidResponse}", resp.IsValidResponse);
            _logger.LogDebug("🔍 Has aggregations: {HasAggregations}", resp.Aggregations != null);

            if (resp.Aggregations == null)
            {
                _logger.LogWarning("❌ No aggregations found in search response");
                return Enumerable.Empty<object>();
            }

            if (!resp.Aggregations.TryGetAggregate<StringTermsAggregate>("top_games", out var termsAgg) || termsAgg == null)
            {
                _logger.LogWarning("❌ No top_games aggregation found in response");
                return Enumerable.Empty<object>();
            }

            _logger.LogInformation("🎯 Found {BucketCount} genre buckets", termsAgg.Buckets.Count);


            var result = termsAgg.Buckets.Select(b => new
            {
                Genre = b.Key,
                Count = b.DocCount
            }).ToList();

            _logger.LogInformation("✅ Returning {ResultCount} popular games aggregation results", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in GetPopularGamesAggregationAsync: {ErrorMessage}", ex.Message);
            return Enumerable.Empty<object>();
        }
    }

    public async Task IndexAsync(GameProjection projection, CancellationToken ct = default)
    {
        if (projection == null) throw new ArgumentNullException(nameof(projection));

        _logger.LogInformation("📝 Indexing game: {GameName} (ID: {GameId})", projection.Name, projection.Id);

        var response = await _client.IndexAsync(projection, i => i
            .Index(_options.IndexName)
            .Id(projection.Id), ct);

        if (response.IsValidResponse)
        {
            _logger.LogInformation("✅ Game {GameName} indexed successfully", projection.Name);
        }
        else
        {
            _logger.LogError("❌ Failed to index game {GameName}: {DebugInformation}",
                projection.Name, response.DebugInformation);
        }
    }

    public async Task<SimpleSearchResult<GameProjection>> SearchAsync(string query, int size = 20, CancellationToken ct = default)
    {
        _logger.LogInformation("🔍 Searching games with query: '{Query}' (size: {Size})", query, size);

        var resp = await _client.SearchAsync<GameProjection>(s => s
            .Indices(_options.IndexName)
            .Size(size)
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(new[] { "name", "description" })
                    .Query(query)
                    .Fuzziness(new Fuzziness("AUTO"))
                )
            ), ct);

        var hits = resp.Hits?.Select(h => h.Source!).Where(x => x != null).ToArray()
                   ?? Array.Empty<GameProjection>();

        long total = resp.Total != 0 ? resp.Total : hits.LongLength;

        _logger.LogInformation("🔍 Search completed: {HitCount} hits found (total: {Total})", hits.Length, total);

        if (!resp.IsValidResponse)
        {
            _logger.LogWarning("⚠️ Search response not valid: {DebugInformation}", resp.DebugInformation);
        }

        return new SimpleSearchResult<GameProjection>(hits, total);
    }

    public async Task UpdateAsync(Guid id, object patch, CancellationToken ct = default)
    {
        _logger.LogInformation("🔄 Updating game with ID: {GameId}", id);

        var response = await _client.UpdateAsync<GameProjection, object>(
            _options.IndexName,
            id,
            u => u.Doc(patch),
            ct
        );

        if (response.IsValidResponse)
        {
            _logger.LogInformation("✅ Game {GameId} updated successfully", id);
        }
        else
        {
            _logger.LogError("❌ Failed to update game {GameId}: {DebugInformation}",
                id, response.DebugInformation);
        }
    }
}