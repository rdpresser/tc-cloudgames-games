using TC.CloudGames.Games.Search;

namespace TC.CloudGames.Games.Api.Endpoints;

public class ReindexGamesEndpoint : EndpointWithoutRequest
{
    private readonly IGameSearchService _search;
    private readonly IDocumentStore _store;

    public ReindexGamesEndpoint(IGameSearchService search, IDocumentStore store)
    {
        _search = search;
        _store = store;
    }

    public override void Configure()
    {
        Post("/games/reindex");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        using var session = _store.QuerySession();
        var games = await session.Query<GameProjection>().ToListAsync(ct);
        await _search.BulkIndexAsync(games, ct);
        await Send.OkAsync(new { message = "Reindex completed", count = games.Count }, cancellation: ct);
    }
}