namespace TC.CloudGames.Games.Search.Events;

/// <summary>
/// Integration event fired when a game is deleted.
/// Triggers removal from the search index.
/// </summary>
public class GameDeletedIntegrationEvent
{
    public string Id { get; set; } = default!;
    public string? GameName { get; set; }
    public DateTimeOffset DeletedAt { get; set; }
}