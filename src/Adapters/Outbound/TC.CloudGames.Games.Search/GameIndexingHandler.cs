using TC.CloudGames.Games.Infrastructure.Projections;
using TC.CloudGames.Games.Search.Events;

namespace TC.CloudGames.Games.Search;

public class GameIndexingHandler
{
    private readonly IGameSearchService _searchService;


    public GameIndexingHandler(IGameSearchService searchService)
    {
        _searchService = searchService;
    }


    public async Task Handle(GameCreatedIntegrationEvent evt)
    {
        var projection = new GameProjection
        {
            Id = evt.Id,
            Name = evt.Name,
            Description = evt.Description,
            Genre = evt.Genre,
            Platforms = evt.Platforms?.ToArray() ?? System.Array.Empty<string>(),
            PlayerCount = evt.PlayerCount,
            ReleaseDate = evt.ReleaseDate,
            IsActive = true
        };


        await _searchService.IndexAsync(projection);
    }


    public async Task Handle(GameUpdatedIntegrationEvent evt)
    {
        var patch = new
        {
            Name = evt.Name,
            Description = evt.Description,
            Genre = evt.Genre,
            Platforms = evt.Platforms,
            ReleaseDate = evt.ReleaseDate
        };


        await _searchService.UpdateAsync(evt.Id, patch);
    }


    public async Task Handle(GameDeletedIntegrationEvent evt)
    {
        await _searchService.DeleteAsync(evt.Id);
    }


    public async Task Handle(GamePlayedIntegrationEvent evt)
    {
        var patch = new { PlayerCount = evt.Delta };
        await _searchService.UpdateAsync(evt.Id, patch);
    }
}