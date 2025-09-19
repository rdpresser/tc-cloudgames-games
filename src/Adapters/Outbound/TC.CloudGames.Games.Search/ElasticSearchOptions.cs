namespace TC.CloudGames.Games.Search;

public class ElasticSearchOptions
{
    public string Url { get; set; } = "http://localhost:9200";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string IndexPrefix { get; set; } = "games";
    public string IndexName => IndexPrefix + "-v1";
}