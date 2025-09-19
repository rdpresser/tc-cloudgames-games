namespace TC.CloudGames.Games.Search;

public class GamesIndexInitializer
{
    private readonly IGameSearchService _svc;

    public GamesIndexInitializer(IGameSearchService svc)
    {
        _svc = svc;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await _svc.EnsureIndexAsync(ct);
    }
}