namespace TC.CloudGames.Games.Search;

public class ElasticSearchOptions
{
    public string? Host { get; set; }
    public int Port { get; set; } = 9200;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? IndexPrefix { get; set; }
    public string IndexName => IndexPrefix + "-v1";
    
    /// <summary>
    /// Gets the full URL for Elasticsearch connection
    /// </summary>
    public string GetConnectionUrl()
    {
        if (!string.IsNullOrEmpty(Host))
            return $"http://{Host}:{Port}";
            
        return "http://localhost:9200";
    }
}