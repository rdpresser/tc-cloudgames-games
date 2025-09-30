# Elasticsearch Configuration Examples

## Production (Elasticsearch Cloud Serverless)

### appsettings.Production.json
```json
{
  "Elasticsearch": {
    "ProjectId": "your-production-project-id",
    "ApiKey": "your-production-api-key",
    "Region": "us-east-1",
    "IndexPrefix": "games-prod",
    "RequestTimeoutSeconds": 30,
    "MaxSearchSize": 1000
  }
}
```

### Environment Variables (Kubernetes/Docker)
```yaml
# kubernetes-deployment.yaml
env:
  - name: ELASTICSEARCH__PROJECTID
    valueFrom:
      secretKeyRef:
        name: elasticsearch-secret
        key: project-id
  - name: ELASTICSEARCH__APIKEY
    valueFrom:
      secretKeyRef:
        name: elasticsearch-secret
        key: api-key
  - name: ELASTICSEARCH__REGION
    value: "us-east-1"
  - name: ELASTICSEARCH__INDEXPREFIX
    value: "games-prod"
```

```bash
# Docker environment file (.env)
ELASTICSEARCH__PROJECTID=your-project-id
ELASTICSEARCH__APIKEY=your-api-key
ELASTICSEARCH__REGION=us-east-1
ELASTICSEARCH__INDEXPREFIX=games-prod
```

## Staging (Elasticsearch Cloud Serverless)

### appsettings.Staging.json
```json
{
  "Elasticsearch": {
    "ProjectId": "your-staging-project-id", 
    "ApiKey": "your-staging-api-key",
    "Region": "us-west-2",
    "IndexPrefix": "games-staging",
    "RequestTimeoutSeconds": 45,
    "MaxSearchSize": 500
  }
}
```

## Development (Local Docker)

### appsettings.Development.json
```json
{
  "Elasticsearch": {
    "Host": "localhost",
    "Port": 9200,
    "Username": "elastic",
    "Password": "elastic123",
    "IndexPrefix": "games-dev",
    "RequestTimeoutSeconds": 60,
    "MaxSearchSize": 100
  }
}
```

### docker-compose.yml (for local development)
```yaml
version: '3.8'
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    container_name: elasticsearch-dev
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=true
      - ELASTIC_PASSWORD=elastic123
      - xpack.security.http.ssl.enabled=false
      - xpack.security.transport.ssl.enabled=false
    ports:
      - "9200:9200"
      - "9300:9300"
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data
    networks:
      - elastic

volumes:
  elasticsearch-data:
    driver: local

networks:
  elastic:
    driver: bridge
```

## Testing (In-Memory or TestContainers)

### appsettings.Test.json
```json
{
  "Elasticsearch": {
    "Host": "localhost",
    "Port": 9299,
    "IndexPrefix": "games-test",
    "RequestTimeoutSeconds": 10,
    "MaxSearchSize": 50
  }
}
```

### TestContainers Example (C#)
```csharp
public class ElasticsearchTestFixture : IAsyncLifetime
{
    private readonly ElasticsearchContainer _container;

    public ElasticsearchTestFixture()
    {
        _container = new ElasticsearchBuilder()
            .WithImage("docker.elastic.co/elasticsearch/elasticsearch:8.11.0")
            .WithPassword("test123")
            .Build();
    }

    public ElasticSearchOptions GetTestOptions()
    {
        return new ElasticSearchOptions
        {
            Host = _container.Hostname,
            Port = _container.GetMappedPublicPort(9200),
            Username = "elastic",
            Password = "test123",
            IndexPrefix = "games-test"
        };
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
```

## Configuration Priority

The configuration system follows this priority order (highest to lowest):

1. **Environment Variables** (e.g., `ELASTICSEARCH__PROJECTID`)
2. **appsettings.{Environment}.json** (e.g., `appsettings.Production.json`)
3. **appsettings.json** (base configuration)
4. **Default values** (in `ElasticSearchOptions` class)

## Security Best Practices

### 1. Never commit secrets to source control
```gitignore
# .gitignore
appsettings.*.json
!appsettings.json
!appsettings.Development.json
.env
*.secrets.json
```

### 2. Use Azure Key Vault or similar for production
```csharp
// Program.cs
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri("https://your-keyvault.vault.azure.net/"),
        new DefaultAzureCredential());
}
```

### 3. Rotate API keys regularly
- Set up automated key rotation
- Monitor API key usage
- Use separate keys for different environments

## Monitoring Configuration

### Application Insights
```json
{
  "ApplicationInsights": {
    "ConnectionString": "your-connection-string"
  },
  "Logging": {
    "LogLevel": {
      "TC.CloudGames.Games.Search": "Information",
      "Elastic.Clients.Elasticsearch": "Warning"
    }
  }
}
```

### Health Checks
```csharp
// Program.cs
services.AddHealthChecks()
    .AddElasticsearch(options => 
    {
        options.Uri = elasticOptions.GetConnectionUrl();
        if (elasticOptions.IsServerless)
            options.ApiKey = elasticOptions.ApiKey;
    });
```

## Performance Tuning

### Production Optimization
```json
{
  "Elasticsearch": {
    "RequestTimeoutSeconds": 30,
    "MaxSearchSize": 1000,
    "IndexPrefix": "games",
    // Additional performance settings
    "BulkIndexingBatchSize": 100,
    "MaxRetryAttempts": 3,
    "RetryDelay": "00:00:05"
  }
}
```

### Development Debug Mode
```json
{
  "Elasticsearch": {
    "RequestTimeoutSeconds": 120,
    "MaxSearchSize": 10,
    "IndexPrefix": "games-debug",
    "EnableDetailedLogging": true,
    "PrettyJson": true
  }
}
```