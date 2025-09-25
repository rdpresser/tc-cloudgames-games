using TC.CloudGames.Games.Search;

namespace TC.CloudGames.Games.Api.Endpoints;

public class PopularGamesEndpoint : EndpointWithoutRequest
{
    private readonly IGameSearchService _search;

    public PopularGamesEndpoint(IGameSearchService search)
    {
        _search = search;
    }

    public override void Configure()
    {
        Get("game/popular");
        Roles(AppConstants.AdminRole);

        Summary(s =>
        {
            s.Summary = "Endpoint for index a new game.";
            s.Description = "This endpoint return popular games.";
            s.Responses[200] = "Returned when the popular game list is successfully.";
            s.Responses[400] = "Returned when a bad request occurs.";
            s.Responses[403] = "Returned when the caller lacks the required role to access this endpoint.";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _search.GetPopularGamesAggregationAsync(10, ct);
        await HttpContext.Response.WriteAsJsonAsync(result, ct);
    }
}