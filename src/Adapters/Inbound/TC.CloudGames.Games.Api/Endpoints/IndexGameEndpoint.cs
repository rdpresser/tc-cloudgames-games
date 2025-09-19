using TC.CloudGames.Games.Search;

namespace TC.CloudGames.Games.Api.Endpoints;

public class IndexGameEndpoint : Endpoint<GameProjection>
{
    private readonly IGameSearchService _search;

    public IndexGameEndpoint(IGameSearchService search)
    {
        _search = search;
    }

    public override void Configure()
    {
        Post("/games/index");
        AllowAnonymous(); // só para teste
    }

    public override async Task HandleAsync(GameProjection req, CancellationToken ct)
    {
        await _search.IndexAsync(req, ct);
        await Send.OkAsync(cancellation: ct);
    }
}