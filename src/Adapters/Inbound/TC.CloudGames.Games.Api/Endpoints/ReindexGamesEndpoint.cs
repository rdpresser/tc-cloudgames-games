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
        Post("game/reindex");
        Roles(AppConstants.AdminRole);

        Summary(s =>
        {
            s.Summary = "Endpoint for reindex all game.";
            s.Description = "This endpoint allows for the creation all games on index.";
            s.Responses[201] = "Returned when a index is successfully created.";
            s.Responses[400] = "Returned when a bad request occurs.";
            s.Responses[403] = "Returned when the caller lacks the required role to access this endpoint.";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        using var session = _store.QuerySession();
        var games = await session.Query<GameProjection>().ToListAsync(ct);
        await _search.BulkIndexAsync(games, ct);
        await Send.OkAsync(new { message = "Reindex completed", count = games.Count }, cancellation: ct);
    }
}