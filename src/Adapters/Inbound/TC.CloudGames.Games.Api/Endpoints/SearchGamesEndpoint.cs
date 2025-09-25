using TC.CloudGames.Games.Search;

namespace TC.CloudGames.Games.Api.Endpoints;

public class SearchGamesEndpoint : Endpoint<SearchRequest>
{
    private readonly IGameSearchService _search;

    public SearchGamesEndpoint(IGameSearchService search)
    {
        _search = search;
    }

    public override void Configure()
    {
        Get("game/search");
        Roles(AppConstants.AdminRole);

        Summary(s =>
        {
            s.Summary = "Endpoint for index a new game.";
            s.Responses[200] = "Returned when the game list is successfully retrieved using the specified filters.";
            s.Responses[201] = "Returned when a new game is successfully created.";
            s.Responses[400] = "Returned when a bad request occurs.";
            s.Responses[403] = "Returned when the caller lacks the required role to access this endpoint.";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
        });
    }

    public override async Task HandleAsync(SearchRequest req, CancellationToken ct)
    {
        var result = await _search.SearchAsync(req.Query, req.Size, ct);
        await HttpContext.Response.WriteAsJsonAsync(result, cancellationToken: ct);
    }
}

public record SearchRequest(string Query, int Size = 20);
