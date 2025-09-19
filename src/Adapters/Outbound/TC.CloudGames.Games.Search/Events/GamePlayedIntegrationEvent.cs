namespace TC.CloudGames.Games.Search.Events;

public class GamePlayedIntegrationEvent 
{ 
    public Guid Id { get; set; } 
    public long Delta { get; set; } 
}