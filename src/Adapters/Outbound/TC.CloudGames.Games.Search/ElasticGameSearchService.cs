using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using TC.CloudGames.Games.Infrastructure.Projections;

namespace TC.CloudGames.Games.Search;

public class ElasticGameSearchService : IGameSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticSearchOptions _options;

    public ElasticGameSearchService(ElasticsearchClient client, ElasticSearchOptions options)
    {
        _client = client;
        _options = options;
    }

    public async Task BulkIndexAsync(IEnumerable<GameProjection> games, CancellationToken ct = default)
    {
        var operations = new List<IBulkOperation>();
        foreach (var g in games)
        {
            operations.Add(new BulkIndexOperation<GameProjection>(g) { Id = g.Id });
        }

        var bulkReq = new BulkRequest
        {
            Index = _options.IndexName,
            Operations = operations
        };

        await _client.BulkAsync(bulkReq, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var request = new DeleteRequest(_options.IndexName, id);
        await _client.DeleteAsync(request, ct);
    }

    public async Task EnsureIndexAsync(CancellationToken ct = default)
    {
        var exists = await _client.Indices.ExistsAsync(_options.IndexName, ct);
        if (exists.Exists) return;

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
    }

    public async Task<IEnumerable<object>> GetPopularGamesAggregationAsync(int size = 10, CancellationToken ct = default)
    {
        var resp = await _client.SearchAsync<GameProjection>(s => s
            .Indices(_options.IndexName)
            .Size(0)
            .Aggregations(a => a
                .Add("top_games", new TermsAggregation
                {
                    Field = "genre.keyword",
                    Size = size
                })
            ), ct);

        if (resp.Aggregations == null)
            return Enumerable.Empty<object>();

        if (!resp.Aggregations.TryGetAggregate<StringTermsAggregate>("top_games", out var termsAgg) || termsAgg == null)
            return Enumerable.Empty<object>();

        return termsAgg.Buckets.Select(b => new
        {
            Genre = b.Key,
            Count = b.DocCount
        });
        }

    public async Task IndexAsync(GameProjection projection, CancellationToken ct = default)
    {
        if (projection == null) throw new ArgumentNullException(nameof(projection));
        await _client.IndexAsync(projection, i => i
            .Index(_options.IndexName)
            .Id(projection.Id), ct);
    }

    public async Task<SimpleSearchResult<GameProjection>> SearchAsync(string query, int size = 20, CancellationToken ct = default)
    {
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

        return new SimpleSearchResult<GameProjection>(hits, total);
    }

    public async Task UpdateAsync(Guid id, object patch, CancellationToken ct = default)
    {
        await _client.UpdateAsync<GameProjection, object>(
            _options.IndexName,
            id,
            u => u.Doc(patch),
            ct
        );
    }

}