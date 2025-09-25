namespace TC.CloudGames.Games.Search;

public class ElasticSearchOptions
{
    public string? Url { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? IndexPrefix { get; set; }
    public string IndexName => IndexPrefix + "-v1";
}