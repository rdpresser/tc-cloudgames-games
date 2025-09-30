namespace TC.CloudGames.Games.Search.Events;

/// <summary>
/// Integration event fired when a new game is created.
/// Contains all game data needed for search indexing.
/// </summary>
public class GameCreatedIntegrationEvent
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public IEnumerable<string>? Platforms { get; set; }
    public string? Developer { get; set; }
    public string? Publisher { get; set; }
    public decimal DiskSizeInGb { get; set; }
    public decimal PriceAmount { get; set; }
    public string? AgeRating { get; set; }
    public string? GameMode { get; set; }
    public string? DistributionFormat { get; set; }
    public string? AvailableLanguages { get; set; }
    public bool SupportsDlcs { get; set; }
    public string? MinimumSystemRequirements { get; set; }
    public string? RecommendedSystemRequirements { get; set; }
    public int? PlaytimeHours { get; set; }
    public int? PlayerCount { get; set; }
    public decimal? RatingAverage { get; set; }
    public string? OfficialLink { get; set; }
    public string? GameStatus { get; set; }
    public string? Tags { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}