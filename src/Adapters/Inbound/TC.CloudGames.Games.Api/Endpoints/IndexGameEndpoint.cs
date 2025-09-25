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
        Post("game/index");
        Roles(AppConstants.AdminRole);

        Summary(s =>
        {
            s.Summary = "Endpoint for index a new game.";
            s.Description = "This endpoint allows for the creation of a new game on index.";
            s.Responses[201] = "Returned when a new game is successfully created.";
            s.Responses[400] = "Returned when a bad request occurs.";
            s.Responses[403] = "Returned when the caller lacks the required role to access this endpoint.";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
        });
    }

    public override async Task HandleAsync(GameProjection req, CancellationToken ct)
    {
        await _search.IndexAsync(req, ct);
        await Send.OkAsync(cancellation: ct);
    }
}