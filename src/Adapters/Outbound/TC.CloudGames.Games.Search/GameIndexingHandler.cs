using Microsoft.Extensions.Logging;
using TC.CloudGames.Games.Infrastructure.Projections;
using TC.CloudGames.Games.Search.Events;

namespace TC.CloudGames.Games.Search;

/// <summary>
/// Handles integration events for game indexing operations.
/// Processes domain events and updates the search index accordingly.
/// </summary>
public class GameIndexingHandler
{
    private readonly IGameSearchService _searchService;
    private readonly ILogger<GameIndexingHandler> _logger;

    public GameIndexingHandler(
        IGameSearchService searchService,
        ILogger<GameIndexingHandler> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Handles game creation events by indexing the new game.
    /// </summary>
    /// <param name="evt">Game created integration event</param>
    public async Task Handle(GameCreatedIntegrationEvent evt)
    {
        try
        {
            _logger.LogInformation("🎮 Processing GameCreated event for game: {GameName} (ID: {GameId})", evt.Name, evt.Id);

            var projection = new GameProjection
            {
                Id = evt.Id,
                Name = evt.Name ?? string.Empty,
                Description = evt.Description,
                Genre = evt.Genre,
                Platforms = evt.Platforms?.ToArray() ?? Array.Empty<string>(),
                Developer = evt.Developer ?? string.Empty,
                Publisher = evt.Publisher,
                DiskSizeInGb = evt.DiskSizeInGb,
                PriceAmount = evt.PriceAmount,
                AgeRating = evt.AgeRating ?? string.Empty,
                GameMode = evt.GameMode ?? string.Empty,
                DistributionFormat = evt.DistributionFormat ?? string.Empty,
                AvailableLanguages = evt.AvailableLanguages,
                SupportsDlcs = evt.SupportsDlcs,
                MinimumSystemRequirements = evt.MinimumSystemRequirements ?? string.Empty,
                RecommendedSystemRequirements = evt.RecommendedSystemRequirements,
                PlaytimeHours = evt.PlaytimeHours,
                PlayerCount = evt.PlayerCount,
                RatingAverage = evt.RatingAverage,
                OfficialLink = evt.OfficialLink,
                GameStatus = evt.GameStatus,
                Tags = evt.Tags,
                ReleaseDate = evt.ReleaseDate,
                CreatedAt = evt.CreatedAt,
                UpdatedAt = null,
                IsActive = true
            };

            await _searchService.IndexAsync(projection);
            _logger.LogInformation("✅ Successfully indexed game: {GameName}", evt.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling GameCreated event for game {GameId}: {ErrorMessage}", evt.Id, ex.Message);
            // Re-throw with context to allow higher-level handlers to decide on retry logic
            throw new InvalidOperationException($"Failed to handle GameCreated event for game {evt.Id}", ex);
        }
    }

    /// <summary>
    /// Handles game update events by updating the search index.
    /// </summary>
    /// <param name="evt">Game updated integration event</param>
    public async Task Handle(GameUpdatedIntegrationEvent evt)
    {
        try
        {
            _logger.LogInformation("🔄 Processing GameUpdated event for game: {GameName} (ID: {GameId})", evt.Name, evt.Id);

            var patch = new
            {
                Name = evt.Name,
                Description = evt.Description,
                Genre = evt.Genre,
                Platforms = evt.Platforms?.ToArray(),
                Developer = evt.Developer,
                Publisher = evt.Publisher,
                DiskSizeInGb = evt.DiskSizeInGb,
                PriceAmount = evt.PriceAmount,
                GameMode = evt.GameMode,
                DistributionFormat = evt.DistributionFormat,
                AvailableLanguages = evt.AvailableLanguages,
                SupportsDlcs = evt.SupportsDlcs,
                MinimumSystemRequirements = evt.MinimumSystemRequirements,
                RecommendedSystemRequirements = evt.RecommendedSystemRequirements,
                PlaytimeHours = evt.PlaytimeHours,
                PlayerCount = evt.PlayerCount,
                RatingAverage = evt.RatingAverage,
                OfficialLink = evt.OfficialLink,
                GameStatus = evt.GameStatus,
                Tags = evt.Tags,
                ReleaseDate = evt.ReleaseDate,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _searchService.UpdateAsync(evt.Id, patch);
            _logger.LogInformation("✅ Successfully updated game index: {GameName}", evt.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling GameUpdated event for game {GameId}: {ErrorMessage}", evt.Id, ex.Message);
            // Re-throw with context to allow higher-level handlers to decide on retry logic
            throw new InvalidOperationException($"Failed to handle GameUpdated event for game {evt.Id}", ex);
        }
    }

    /// <summary>
    /// Handles game deletion events by removing the game from the search index.
    /// </summary>
    /// <param name="evt">Game deleted integration event</param>
    public async Task Handle(GameDeletedIntegrationEvent evt)
    {
        try
        {
            _logger.LogInformation("🗑️ Processing GameDeleted event for game ID: {GameId}", evt.Id);

            await _searchService.DeleteAsync(evt.Id);
            _logger.LogInformation("✅ Successfully deleted game from index: {GameId}", evt.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling GameDeleted event for game {GameId}: {ErrorMessage}", evt.Id, ex.Message);
            // Re-throw with context to allow higher-level handlers to decide on retry logic
            throw new InvalidOperationException($"Failed to handle GameDeleted event for game {evt.Id}", ex);
        }
    }

    /// <summary>
    /// Handles game played events by updating player count statistics.
    /// </summary>
    /// <param name="evt">Game played integration event</param>
    public async Task Handle(GamePlayedIntegrationEvent evt)
    {
        try
        {
            _logger.LogInformation("🎯 Processing GamePlayed event for game ID: {GameId} (Delta: {Delta})", evt.Id, evt.Delta);

            var patch = new
            {
                PlayerCount = evt.Delta,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _searchService.UpdateAsync(evt.Id, patch);
            _logger.LogInformation("✅ Successfully updated player count for game: {GameId}", evt.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling GamePlayed event for game {GameId}: {ErrorMessage}", evt.Id, ex.Message);
            // Re-throw with context to allow higher-level handlers to decide on retry logic
            throw new InvalidOperationException($"Failed to handle GamePlayed event for game {evt.Id}", ex);
        }
    }

    /// <summary>
    /// Handles bulk game indexing from game projections.
    /// Useful for initial data loading or reindexing operations.
    /// </summary>
    /// <param name="gameProjections">Collection of game projections to index</param>
    public async Task HandleBulkIndex(IEnumerable<GameProjection> gameProjections)
    {
        try
        {
            var projections = gameProjections.ToList();
            _logger.LogInformation("📦 Processing bulk index for {GameCount} games", projections.Count);

            if (projections.Any())
            {
                await _searchService.BulkIndexAsync(projections);
                _logger.LogInformation("✅ Successfully bulk indexed {GameCount} games", projections.Count);
            }
            else
            {
                _logger.LogInformation("📦 No games to index in bulk operation");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in bulk index operation: {ErrorMessage}", ex.Message);
            // Re-throw with context to allow higher-level handlers to decide on retry logic
            throw new InvalidOperationException("Failed to perform bulk index operation", ex);
        }
    }
}