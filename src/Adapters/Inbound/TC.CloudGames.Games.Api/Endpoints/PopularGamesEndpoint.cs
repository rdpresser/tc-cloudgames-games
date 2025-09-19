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
        Get("/games/popular");
        AllowAnonymous(); // só para teste
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _search.GetPopularGamesAggregationAsync(10, ct);
        await HttpContext.Response.WriteAsJsonAsync(result, ct);
    }
}