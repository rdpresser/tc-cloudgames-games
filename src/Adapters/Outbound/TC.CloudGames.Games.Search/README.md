# TC.CloudGames.Games.Search

Elasticsearch integration for game search functionality, optimized for **Elasticsearch Cloud** with support for **Elasticsearch Cloud Serverless** and local development.

## Features

- ? **Elasticsearch Cloud** support with API Key authentication and direct URL
- ? **Elasticsearch Cloud Serverless** support with ProjectId and Region
- ? **Local development** support with Docker/basic authentication
- ? **Full-text search** with fuzzy matching and field boosting
- ? **Advanced filtering** by genre, platform, price, rating, and release date
- ? **Bulk indexing** for efficient data loading
- ? **Aggregations** for popular genres and analytics
- ? **Automatic index management** (Cloud compatible)
- ? **Production-ready** error handling and logging

## Configuration

### Elasticsearch Cloud (Regular) - Recommended for Production

For your Elasticsearch Cloud instance:

```json
{
  "Elasticsearch": {
    "EndpointUrl": "https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443",
    "ApiKey": "YOUR_API_KEY_HERE",
    "IndexPrefix": "search-xn8c",
    "RequestTimeoutSeconds": 30,
    "MaxSearchSize": 1000
  }
}
```

**To get your API Key:**
1. Go to your [Elasticsearch Cloud Console](https://cloud.elastic.co/)
2. Select your deployment
3. Go to Security ? API Keys
4. Create a new API key or copy an existing one
5. Use the base64-encoded key in the configuration

### Elasticsearch Cloud Serverless

```json
{
  "Elasticsearch": {
    "ProjectId": "your-project-id",
    "ApiKey": "your-base64-encoded-api-key",
    "Region": "us-east-1",
    "IndexPrefix": "search-xn8c"
  }
}
```

### Local Development (Docker)

```json
{
  "Elasticsearch": {
    "Host": "localhost",
    "Port": 9200,
    "Username": "elastic", 
    "Password": "your-password",
    "IndexPrefix": "search-xn8c-dev"
  }
}
```

### Environment Variables

You can also use environment variables (useful for Docker/Kubernetes):

```bash
# Elasticsearch Cloud
ELASTICSEARCH__ENDPOINTURL=https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443
ELASTICSEARCH__APIKEY=your-api-key
ELASTICSEARCH__INDEXPREFIX=search-xn8c

# Serverless
ELASTICSEARCH__PROJECTID=your-project-id
ELASTICSEARCH__APIKEY=your-api-key
ELASTICSEARCH__REGION=us-east-1

# Local
ELASTICSEARCH__HOST=localhost
ELASTICSEARCH__PORT=9200
ELASTICSEARCH__USERNAME=elastic
ELASTICSEARCH__PASSWORD=your-password
```

## Quick Start Guide

### 1. Configure Your Elasticsearch Connection

Update your `appsettings.json` or `appsettings.Development.json`:

```json
{
  "Elasticsearch": {
    "EndpointUrl": "https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443",
    "ApiKey": "YOUR_ACTUAL_API_KEY",
    "IndexPrefix": "search-xn8c"
  }
}
```

### 2. Test the Connection

Run your API and check the logs. You should see:
```
?? Elasticsearch configured for Cloud deployment
?? Connecting to: https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443
```

### 3. Index Your First Game

Use the reindex endpoint to load existing games:
```bash
POST /game/reindex
```

### 4. Search for Games

```bash
GET /games/search?query=action
```

## Usage

### Dependency Injection Setup

```csharp
// In Program.cs or Startup.cs
services.AddElasticSearch(configuration);
```

### Basic Search

```csharp
public class GameController : ControllerBase
{
    private readonly IGameSearchService _searchService;

    public GameController(IGameSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string query, int size = 20)
    {
        var results = await _searchService.SearchAsync(query, size);
        return Ok(results);
    }
}
```

### Advanced Search with Filters

```csharp
[HttpPost("search/advanced")]
public async Task<IActionResult> AdvancedSearch([FromBody] GameSearchRequest request)
{
    var results = await _searchService.SearchAdvancedAsync(request);
    return Ok(results);
}
```

### Example Advanced Search Request

```json
{
  "query": "action adventure",
  "genres": ["Action", "Adventure"],
  "platforms": ["PC", "PlayStation 5"],
  "minPrice": 10.00,
  "maxPrice": 60.00,
  "minRating": 4.0,
  "releaseDateFrom": "2020-01-01",
  "releaseDateTo": "2024-12-31",
  "size": 20,
  "from": 0,
  "sortBy": "releaseDate",
  "sortDirection": "desc"
}
```

### Indexing Games

```csharp
// Index single game
await _searchService.IndexAsync(gameProjection);

// Bulk index multiple games
await _searchService.BulkIndexAsync(gameProjections);

// Update game
await _searchService.UpdateAsync(gameId, new { name = "New Name" });

// Delete game
await _searchService.DeleteAsync(gameId.ToString());
```

### Popular Genres Aggregation

```csharp
[HttpGet("popular-genres")]
public async Task<IActionResult> GetPopularGenres(int size = 10)
{
    var genres = await _searchService.GetPopularGamesAggregationAsync(size);
    return Ok(genres);
}
```

## Search Features

### Full-Text Search
- Searches across: **name** (3x boost), **description** (2x boost), **genre**, **developer**, **tags**
- **Fuzzy matching** for typo tolerance
- **Auto-completion** friendly

### Advanced Filtering
- **Genre**: Exact match filtering
- **Platform**: Multi-platform filtering  
- **Price Range**: Min/max price filtering
- **Rating**: Minimum rating filtering
- **Release Date**: Date range filtering

### Sorting Options
- **Relevance** (default)
- **Name** (alphabetical)
- **Release Date**
- **Price**
- **Rating**
- **Creation Date**

## Architecture

```
???????????????????    ???????????????????    ???????????????????
?   Game Events   ?????? GameProjection  ??????  Elasticsearch  ?
?  (Event Store)  ?    ?   (Read Model)  ?    ?   (Cloud)       ?
???????????????????    ???????????????????    ???????????????????
                              ?
                              ?
                       ???????????????????
                       ? Search Service  ?
                       ? (This Project)  ?
                       ???????????????????
```

## Performance

### Elasticsearch Cloud Benefits
- ? **Managed service**: No infrastructure management
- ? **High availability**: Built-in redundancy and failover
- ? **Security**: Built-in security features and encryption
- ? **Monitoring**: Integrated monitoring and alerting
- ? **Scalability**: Easy scaling based on usage

### Best Practices
- Use **bulk indexing** for multiple documents
- Implement **result pagination** for large datasets
- Use **specific field searches** when possible
- Cache **popular queries** at the application level

## Troubleshooting

### Common Issues

**"Invalid Elasticsearch configuration"**
- Ensure EndpointUrl and ApiKey are provided for Elasticsearch Cloud
- Verify API key has proper permissions

**"Connection refused"**
- Check if your EndpointUrl is correct
- Verify your API key is valid and not expired
- Ensure network connectivity to Elasticsearch Cloud

**"Index not found"**
- In Elasticsearch Cloud, run the reindex endpoint: `POST /game/reindex`
- Check if your index name matches the configuration

**"Authentication failed"**
- Verify API key has proper permissions
- Check if the API key is properly base64 encoded

### Logging

The service provides comprehensive logging:
- ?? **Index operations**: Document indexing/updating
- ?? **Search operations**: Query execution and results
- ?? **Aggregations**: Popular genres and analytics
- ? **Errors**: Detailed error information with context

## Migration Guide

### From Local to Elasticsearch Cloud

1. **Update configuration** from Host/Port to EndpointUrl/ApiKey
2. **Test connection** in development environment
3. **Run reindex** to populate the cloud index
4. **Verify search functionality** with your data

### Configuration Changes

**Before (Local):**
```json
{
  "Elasticsearch": {
    "Host": "localhost",
    "Port": 9200,
    "Username": "elastic",
    "Password": "password"
  }
}
```

**After (Elasticsearch Cloud):**
```json
{
  "Elasticsearch": {
    "EndpointUrl": "https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443",
    "ApiKey": "your-api-key"
  }
}
```

No code changes needed in consuming applications!