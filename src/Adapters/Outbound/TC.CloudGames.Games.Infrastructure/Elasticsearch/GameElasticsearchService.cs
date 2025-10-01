namespace TC.CloudGames.Games.Infrastructure.Elasticsearch;

/// <summary>
/// Elasticsearch implementation of game search service.
/// Optimized for Elasticsearch Cloud Serverless with automatic index management.
/// </summary>
public class GameElasticsearchService : IGameElasticsearchService
{
    private readonly IElasticsearchClientProvider _clientProvider;
    private readonly ILogger<GameElasticsearchService> _logger;

    public GameElasticsearchService(IElasticsearchClientProvider clientProvider,
        ILogger<GameElasticsearchService> logger)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task IndexAsync(GameProjection projection, CancellationToken ct = default)
    {
        if (projection == null) throw new ArgumentNullException(nameof(projection));

        _logger.LogInformation("Indexing game: {GameName} (ID: {GameId})", projection.Name, projection.Id);

        try
        {
            var response = await _clientProvider.Client.IndexAsync(projection, i => i
                .Index(_clientProvider.IndexName)
                .Id(projection.Id), ct);

            if (response.IsValidResponse)
            {
                _logger.LogInformation("Game {GameName} indexed successfully", projection.Name);
            }
            else
            {
                _logger.LogError("Failed to index game {GameName}: {DebugInformation}",
                    projection.Name, response.DebugInformation);
                throw new InvalidOperationException($"Failed to index game: {response.DebugInformation}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing game {GameName}: {ErrorMessage}", projection.Name, ex.Message);
            throw new InvalidOperationException($"Error indexing game {projection.Name}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task BulkIndexAsync(IEnumerable<GameProjection> games, CancellationToken ct = default)
    {
        var gamesList = games.ToList();
        if (!gamesList.Any())
        {
            _logger.LogInformation("No games to index in bulk operation");
            return;
        }

        _logger.LogInformation("Bulk indexing {GameCount} games to index: {IndexName}", gamesList.Count, _clientProvider.IndexName);

        try
        {
            var operations = new List<IBulkOperation>();
            foreach (var game in gamesList)
            {
                _logger.LogDebug("Adding game to bulk: {GameName} (Genre: {Genre})", game.Name, game.Genre);
                operations.Add(new BulkIndexOperation<GameProjection>(game) { Id = game.Id });
            }

            var bulkRequest = new BulkRequest
            {
                Index = _clientProvider.IndexName,
                Operations = operations
            };

            var response = await _clientProvider.Client.BulkAsync(bulkRequest, ct);

            if (response.IsValidResponse)
            {
                _logger.LogInformation("Bulk indexing completed successfully for {GameCount} games", gamesList.Count);

                if (response.Errors)
                {
                    var errorCount = response.Items.Count(i => i.Error != null);
                    _logger.LogWarning("Bulk operation had {ErrorCount} errors out of {TotalCount} items", errorCount, gamesList.Count);
                }
            }
            else
            {
                _logger.LogError("Bulk indexing failed: {DebugInformation}", response.DebugInformation);
                throw new InvalidOperationException($"Bulk indexing failed: {response.DebugInformation}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk indexing: {ErrorMessage}", ex.Message);
            throw new InvalidOperationException("Error in bulk indexing operation", ex);
        }
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Guid id, object patch, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating game with ID: {GameId}", id);

        try
        {
            var response = await _clientProvider.Client.UpdateAsync<GameProjection, object>(
                _clientProvider.IndexName,
                id,
                u => u.Doc(patch),
                ct);

            if (response.IsValidResponse)
            {
                _logger.LogInformation("✅ Game {GameId} updated successfully", id);
            }
            else
            {
                _logger.LogError(" Failed to update game {GameId}: {DebugInformation}",
                    id, response.DebugInformation);
                throw new InvalidOperationException($"Failed to update game: {response.DebugInformation}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating game {GameId}: {ErrorMessage}", id, ex.Message);
            throw new InvalidOperationException($"Error updating game {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting game with ID: {GameId} from index: {IndexName}", id, _clientProvider.IndexName);

        try
        {
            var request = new DeleteRequest(_clientProvider.IndexName, id);
            var response = await _clientProvider.Client.DeleteAsync(request, ct);

            if (response.IsValidResponse)
            {
                _logger.LogInformation("Game {GameId} deleted successfully", id);
            }
            else
            {
                _logger.LogWarning("Game {GameId} deletion response: {DebugInformation}", id, response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game {GameId}: {ErrorMessage}", id, ex.Message);
            throw new InvalidOperationException($"Error deleting game {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<SimpleSearchResult<GameProjection>> SearchAsync(string query, int size = 20, CancellationToken ct = default)
    {
        _logger.LogInformation("Searching games with query: '{Query}' (size: {Size})", query, size);

        var searchRequest = new GameSearchRequest
        {
            Query = query,
            Size = Math.Min(size, _clientProvider.MaxSearchSize)
        };

        return await SearchAdvancedAsync(searchRequest, ct);
    }

    /// <inheritdoc/>
    public async Task<SimpleSearchResult<GameProjection>> SearchAdvancedAsync(GameSearchRequest searchRequest, CancellationToken ct = default)
    {
        _logger.LogInformation("Advanced search with query: '{Query}' (size: {Size})", searchRequest.Query, searchRequest.Size);

        try
        {
            var response = await _clientProvider.Client.SearchAsync<GameProjection>(s => s
                .Indices(_clientProvider.IndexName)
                .Size(Math.Min(searchRequest.Size, _clientProvider.MaxSearchSize))
                .From(searchRequest.From)
                .Query(BuildSearchQuery(searchRequest))
                .Sort(BuildSortOptions(searchRequest)), ct);

            var hits = response.Hits?.Select(h => h.Source!).Where(x => x != null).ToArray()
                       ?? Array.Empty<GameProjection>();

            long total = response.Total;

            _logger.LogInformation("Search completed: {HitCount} hits found (total: {Total})", hits.Length, total);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Search response not valid: {DebugInformation}", response.DebugInformation);
            }

            return new SimpleSearchResult<GameProjection>(hits, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in advanced search: {ErrorMessage}", ex.Message);
            throw new InvalidOperationException("Error in advanced search operation", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PopularGenreResult>> GetPopularGamesAggregationAsync(int size = 10, CancellationToken ct = default)
    {
        _logger.LogInformation("🔍 Getting popular games aggregation with size: {Size}", size);

        try
        {
            var response = await _clientProvider.Client.SearchAsync<GameProjection>(s => s
                .Indices(_clientProvider.IndexName)
                .Size(0) // We only want aggregations, no documents
                .Query(q => q.Bool(b => b.Filter(f => f.Term(t => t.Field("isActive").Value(true)))))
                .Aggregations(a => a
                    .Add("popular_genres", new TermsAggregation
                    {
                        Field = "genre.keyword", // Use keyword field for exact matches
                        Size = size,
                        MinDocCount = 1
                    })), ct);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Aggregation search failed: {DebugInformation}", response.DebugInformation);
                return Enumerable.Empty<PopularGenreResult>();
            }

            if (response.Aggregations == null)
            {
                _logger.LogWarning("No aggregations found in search response");
                return Enumerable.Empty<PopularGenreResult>();
            }

            if (!response.Aggregations.TryGetAggregate<StringTermsAggregate>("popular_genres", out var termsAgg) || termsAgg == null)
            {
                _logger.LogWarning("No popular_genres aggregation found in response");
                return Enumerable.Empty<PopularGenreResult>();
            }

            _logger.LogInformation("Found {BucketCount} genre buckets", termsAgg.Buckets.Count);

            var results = termsAgg.Buckets
                .Where(b => !string.IsNullOrEmpty(b.Key.ToString()))
                .Select(b => new PopularGenreResult(b.Key.ToString()!, b.DocCount))
                .ToList();

            _logger.LogInformation("Returning {ResultCount} popular genre results", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPopularGamesAggregationAsync: {ErrorMessage}", ex.Message);
            throw new InvalidOperationException("Error in aggregation operation", ex);
        }
    }

    /// <summary>
    /// Builds the search query based on the search request parameters.
    /// </summary>
    private static Query BuildSearchQuery(GameSearchRequest request)
    {
        var queries = new List<Query>();

        // Base active filter
        queries.Add(new TermQuery { Field = "isActive", Value = true });

        // Text search
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queries.Add(new MultiMatchQuery
            {
                Fields = new[] { "name^3", "description^2", "genre", "developer", "tags" },
                Query = request.Query,
                Fuzziness = new Fuzziness("AUTO"),
                Operator = Operator.Or
            });
        }

        // Genre filters
        if (request.Genres?.Any() == true)
        {
            queries.Add(new TermsQuery
            {
                Field = "genre.keyword",
                Terms = request.Genres.Select(g => (FieldValue)g).ToArray()
            });
        }

        // Platform filters
        if (request.Platforms?.Any() == true)
        {
            queries.Add(new TermsQuery
            {
                Field = "platforms.keyword",
                Terms = request.Platforms.Select(p => (FieldValue)p).ToArray()
            });
        }

        // Price range filters
        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            var rangeQuery = new NumberRangeQuery { Field = "priceAmount" };
            if (request.MinPrice.HasValue)
                rangeQuery.Gte = (double)request.MinPrice.Value;
            if (request.MaxPrice.HasValue)
                rangeQuery.Lte = (double)request.MaxPrice.Value;
            queries.Add(rangeQuery);
        }

        // Rating filter
        if (request.MinRating.HasValue)
        {
            queries.Add(new NumberRangeQuery { Field = "ratingAverage", Gte = (double)request.MinRating.Value });
        }

        // Release date range filters
        if (request.ReleaseDateFrom.HasValue || request.ReleaseDateTo.HasValue)
        {
            var rangeQuery = new DateRangeQuery { Field = "releaseDate" };
            if (request.ReleaseDateFrom.HasValue)
                rangeQuery.Gte = DateMath.Anchored(request.ReleaseDateFrom.Value.ToDateTime(TimeOnly.MinValue));
            if (request.ReleaseDateTo.HasValue)
                rangeQuery.Lte = DateMath.Anchored(request.ReleaseDateTo.Value.ToDateTime(TimeOnly.MaxValue));
            queries.Add(rangeQuery);
        }

        return new BoolQuery { Must = queries.ToArray() };
    }

    /// <summary>
    /// Builds sort options based on the search request.
    /// </summary>
    private static ICollection<SortOptions> BuildSortOptions(GameSearchRequest request)
    {
        var sorts = new List<SortOptions>();

        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var sortOrder = request.SortDirection.ToLowerInvariant() == "asc"
                ? SortOrder.Asc
                : SortOrder.Desc;

            var sortField = request.SortBy.ToLowerInvariant() switch
            {
                "name" => "name.keyword",
                "releasedate" => "releaseDate",
                "price" => "priceAmount",
                "rating" => "ratingAverage",
                "createdat" => "createdAt",
                _ => "_score"
            };

            sorts.Add(new SortOptions
            {
                Field = new FieldSort
                {
                    Field = sortField,
                    Order = sortOrder
                }
            });
        }
        else
        {
            // Default sort by relevance score
            sorts.Add(new SortOptions
            {
                Score = new ScoreSort { Order = SortOrder.Desc }
            });
        }

        return sorts;
    }
}