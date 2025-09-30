namespace TC.CloudGames.Games.Search.Events;

/// <summary>
/// Integration event fired when a game is played.
/// Used to update player count statistics in the search index.
/// </summary>
public class GamePlayedIntegrationEvent
{
    public Guid Id { get; set; }
    public int Delta { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset PlayedAt { get; set; }
}