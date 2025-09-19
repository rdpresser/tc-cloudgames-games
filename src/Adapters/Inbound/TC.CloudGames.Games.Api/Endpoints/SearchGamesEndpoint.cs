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
        Get("/games/search");
        AllowAnonymous(); // só para teste
    }

    public override async Task HandleAsync(SearchRequest req, CancellationToken ct)
    {
        var result = await _search.SearchAsync(req.Query, req.Size, ct);
        await HttpContext.Response.WriteAsJsonAsync(result, cancellationToken: ct);
    }
}

public record SearchRequest(string Query, int Size = 20);
