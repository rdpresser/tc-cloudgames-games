namespace TC.CloudGames.Games.Search.Events;

public class GameCreatedIntegrationEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public IEnumerable<string>? Platforms { get; set; }
    public int? PlayerCount { get; set; }
    public DateOnly ReleaseDate { get; set; }
}