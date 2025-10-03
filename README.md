# 🎮 TC Cloud Games - Games Service

The Games microservice is responsible for managing the game catalog, user game libraries, game purchases, and providing advanced search capabilities for the TC Cloud Games platform.

## 🏗️ Architecture Overview

This service follows **Hexagonal Architecture (Ports & Adapters)** with **Domain-Driven Design (DDD)** principles:
```
TC.CloudGames.Games/
├── 🎯 Core/ # Business Logic
│ ├── Domain/ # Domain Layer
│ │ ├── Aggregates/ # Game, UserGameLibrary
│ │ ├── ValueObjects/ # GameDetails, Price, Rating, etc.
│ │ └── Abstractions/ # Domain Interfaces
│ └── Application/ # Application Layer
│ ├── UseCases/ # CQRS Commands & Queries
│ ├── Ports/ # Application Interfaces
│ └── MessageBrokerHandlers/ # Event Handlers
├── 🔌 Adapters/ # Infrastructure & API
│ ├── Inbound/ # API Layer
│ │ └── TC.CloudGames.Games.Api/ # REST API Endpoints
│ └── Outbound/ # Infrastructure Layer
│ ├── TC.CloudGames.Games.Infrastructure/ # Database & Repositories
│ └── TC.CloudGames.Games.Search/ # Elasticsearch Integration
└── 🧪 test/ # Unit & Integration Tests
└── TC.CloudGames.Games.Unit.Tests/
```
## 🎯 Domain Model

### Core Aggregates

#### 🎮 Game Aggregate
The main aggregate representing a game in the catalog:

```csharp
public sealed class GameAggregate : BaseAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateOnly ReleaseDate { get; private set; }
    public AgeRating AgeRating { get; private set; }
    public DeveloperInfo DeveloperInfo { get; private set; }
    public DiskSize DiskSize { get; private set; }
    public Price Price { get; private set; }
    public Playtime Playtime { get; private set; }
    public GameDetails GameDetails { get; private set; }
    public SystemRequirements SystemRequirements { get; private set; }
    public Rating Rating { get; private set; }
    public string OfficialLink { get; private set; }
    public string GameStatus { get; private set; }
}
```

#### 📚 UserGameLibrary Aggregate
Manages user's game collection and purchase history:

```csharp
public sealed class UserGameLibraryAggregate : BaseAggregateRoot
{
    public Guid UserId { get; private set; }
    public IReadOnlyCollection<GamePurchase> GamePurchases { get; private set; }
    public DateTime LastUpdated { get; private set; }
}
```

### Value Objects

#### 🎯 GameDetails
Rich value object containing game metadata:

- **Platforms**: PC, PlayStation 4/5, Xbox One/Series X|S, Nintendo Switch, etc.
- **Game Modes**: Singleplayer, Multiplayer, Co-op, PvP, etc.
- **Distribution Formats**: Digital, Physical
- **Genre**: Game categorization
- **Tags**: Searchable keywords
- **Languages**: Available languages
- **DLC Support**: Expansion pack support

#### 💰 Price
Monetary value with currency support and validation.

#### ⭐ Rating
Game rating system with validation and aggregation.

#### ⏱️ Playtime
Estimated gameplay duration (main story + completionist).

#### 🖥️ SystemRequirements
Minimum and recommended system specifications.

## 🔧 Technology Stack

### Backend Framework
- **.NET 9**: Modern, high-performance framework
- **FastEndpoints**: Minimalist, high-performance API endpoints
- **FluentValidation**: Comprehensive input validation

### Data & Storage
- **Marten**: Event Store and Document Database for PostgreSQL
- **PostgreSQL**: Primary database with dedicated schema
- **Redis**: Distributed caching and session storage
- **Elasticsearch**: Advanced search and analytics

### Messaging & Communication
- **Wolverine**: Message broker with built-in CQRS support
- **Azure Service Bus**: Cloud messaging for production
- **RabbitMQ**: Local development messaging

### Observability
- **Serilog**: Structured logging with correlation IDs
- **Grafana Loki**: Log aggregation
- **Application Insights**: Performance monitoring
- **Health Checks**: Service health monitoring

## 🚀 API Endpoints

### Game Management

#### Create Game
```http
POST /api/game
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Cyberpunk 2077",
  "releaseDate": "2020-12-10",
  "ageRating": "M",
  "description": "An open-world action-adventure story",
  "developerInfo": {
    "developer": "CD Projekt RED",
    "publisher": "CD Projekt"
  },
  "diskSize": 70.0,
  "price": 59.99,
  "playtime": {
    "mainStory": 25,
    "completionist": 100
  },
  "gameDetails": {
    "genre": "RPG",
    "platforms": ["PC", "PlayStation 5", "Xbox Series X|S"],
    "tags": "cyberpunk, rpg, open-world",
    "gameMode": "Singleplayer",
    "distributionFormat": "Digital",
    "availableLanguages": "English, Portuguese, Spanish",
    "supportsDlcs": true
  },
  "systemRequirements": {
    "minimum": "Intel Core i5-3570K / AMD FX-8310",
    "recommended": "Intel Core i7-4790 / AMD Ryzen 3 3200G"
  },
  "rating": 4.5,
  "officialLink": "https://www.cyberpunk.net",
  "gameStatus": "Available"
}
```

#### Get Game by ID
```http
GET /api/game/{id}
Authorization: Bearer {token}
```

#### Get Game List
```http
GET /api/games?page=1&size=20&genre=RPG&platform=PC
Authorization: Bearer {token}
```

### Game Search

#### Advanced Search
```http
POST /api/games/search
Authorization: Bearer {token}
Content-Type: application/json

{
  "query": "cyberpunk",
  "filters": {
    "genres": ["RPG", "Action"],
    "platforms": ["PC", "PlayStation 5"],
    "priceRange": {
      "min": 0,
      "max": 100
    },
    "rating": {
      "min": 4.0
    }
  },
  "sortBy": "rating",
  "sortOrder": "desc",
  "size": 20
}
```

#### Popular Games
```http
GET /api/games/popular?size=10
Authorization: Bearer {token}
```

### Game Purchases

#### Purchase Game
```http
POST /api/games/{id}/purchase
Authorization: Bearer {token}
Content-Type: application/json

{
  "paymentMethod": "credit_card",
  "billingAddress": {
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "zipCode": "10001",
    "country": "US"
  }
}
```

### Search Index Management

#### Index Game
```http
POST /api/games/{id}/index
Authorization: Bearer {token}
```

#### Reindex All Games
```http
POST /api/games/reindex
Authorization: Bearer {token}
```

## 🏛️ Use Cases

### Commands (Write Operations)

#### CreateGame
- **Purpose**: Add new game to catalog
- **Validation**: Comprehensive input validation
- **Events**: GameCreated domain event
- **Side Effects**: Elasticsearch indexing

#### PurchaseGame
- **Purpose**: Process game purchase
- **Validation**: User authentication, payment validation
- **Events**: GamePurchased domain event
- **Integration**: Payment service communication

### Queries (Read Operations)

#### GetGameById
- **Purpose**: Retrieve specific game details
- **Caching**: Redis caching for performance
- **Projection**: Optimized read model

#### GetGameList
- **Purpose**: List games with filtering and pagination
- **Performance**: Database indexing and query optimization
- **Caching**: Query result caching

#### SearchGames
- **Purpose**: Advanced search with Elasticsearch
- **Features**: Full-text search, faceted search, sorting
- **Performance**: Elasticsearch-powered search

## 🔍 Search Capabilities

### Elasticsearch Integration

The service provides powerful search capabilities through Elasticsearch:

#### Full-Text Search
- Game titles, descriptions, and tags
- Fuzzy matching and typo tolerance
- Multi-language support

#### Faceted Search
- **Genre**: Filter by game genre
- **Platform**: Filter by supported platforms
- **Price Range**: Filter by price brackets
- **Rating**: Filter by user ratings
- **Release Date**: Filter by release period

#### Advanced Features
- **Auto-complete**: Search suggestions
- **Popular Games**: Aggregated popularity metrics
- **Recommendations**: Similar games suggestions
- **Real-time Indexing**: Automatic search index updates

### Search Configuration

```csharp
public class ElasticGameSearchService : IGameSearchService
{
    public async Task<SimpleSearchResult<GameProjection>> SearchAsync(
        string query, 
        int size = 20, 
        CancellationToken ct = default)
    
    public async Task<IEnumerable<object>> GetPopularGamesAggregationAsync(
        int size = 10, 
        CancellationToken ct = default)
}
```

## 📊 Event Sourcing & Projections

### Domain Events

#### GameCreated
```csharp
public sealed record GameCreated(
    Guid GameId,
    string Name,
    string Description,
    DateOnly ReleaseDate,
    string AgeRating,
    DeveloperInfo DeveloperInfo,
    Price Price,
    GameDetails GameDetails,
    DateTime CreatedAt
) : BaseDomainEvent;
```

#### GamePurchased
```csharp
public sealed record GamePurchased(
    Guid GameId,
    Guid UserId,
    Guid PurchaseId,
    Price Price,
    DateTime PurchasedAt
) : BaseDomainEvent;
```

### Projections

#### GameProjection
Read model optimized for search and display:

```csharp
public sealed record GameProjection
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public DateOnly ReleaseDate { get; init; }
    public string AgeRating { get; init; }
    public string Developer { get; init; }
    public string Publisher { get; init; }
    public decimal PriceAmount { get; init; }
    public decimal RatingAverage { get; init; }
    public string Genre { get; init; }
    public IReadOnlyCollection<string> Platforms { get; init; }
    public string Tags { get; init; }
    public string GameMode { get; init; }
    public string DistributionFormat { get; init; }
    public string AvailableLanguages { get; init; }
    public bool SupportsDlcs { get; init; }
    public string GameStatus { get; init; }
}
```

#### UserGameLibraryProjection
User's game collection for quick access:

```csharp
public sealed record UserGameLibraryProjection
{
    public Guid UserId { get; init; }
    public IReadOnlyCollection<GamePurchase> Purchases { get; init; }
    public DateTime LastUpdated { get; init; }
}
```

## 🧪 Testing Strategy

### Unit Tests
- **Domain Logic**: Aggregate behavior and business rules
- **Value Objects**: Validation and immutability
- **Use Cases**: Command and query handlers
- **API Endpoints**: Request/response validation

### Integration Tests
- **Database**: Repository implementations
- **Search**: Elasticsearch integration
- **Messaging**: Event handling
- **API**: End-to-end request processing

### Test Categories

```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=Api

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🚀 Local Development

### Prerequisites
- .NET 9 SDK
- Docker Desktop
- PostgreSQL (or use Docker)
- Redis (or use Docker)
- Elasticsearch (or use Docker)

### Setup

```bash
# Clone and navigate to the games service
cd services/games

# Restore dependencies
dotnet restore

# Run the service
cd src/Adapters/Inbound/TC.CloudGames.Games.Api
dotnet run
```

### Environment Configuration

#### Development
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=games_db;Username=postgres;Password=password"
  },
  "Elasticsearch": {
    "Host": "http://localhost:9200",
    "IndexName": "games"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

## 🔐 Security & Validation

### Input Validation
- **FluentValidation**: Comprehensive request validation
- **Value Objects**: Domain-level validation
- **Sanitization**: Input sanitization and encoding

### Authentication & Authorization
- **JWT Tokens**: Secure API access
- **Role-Based Access**: Admin-only operations
- **API Keys**: Service-to-service communication

### Data Protection
- **Encryption**: Sensitive data encryption
- **Audit Trail**: Complete operation logging
- **Rate Limiting**: API abuse prevention

## 📈 Performance & Monitoring

### Caching Strategy
- **Redis**: Query result caching
- **Application Cache**: In-memory caching
- **CDN**: Static content delivery

### Database Optimization
- **Indexing**: Strategic database indexes
- **Query Optimization**: Efficient data retrieval
- **Connection Pooling**: Database connection management

### Monitoring
- **Application Insights**: Performance metrics
- **Health Checks**: Service health monitoring
- **Logging**: Structured logging with correlation IDs

## 🔄 Integration Points

### External Services
- **Users Service**: User authentication and validation
- **Payments Service**: Payment processing integration
- **Search Service**: Elasticsearch indexing and search

### Message Contracts
- **GameCreated**: Notify other services of new games
- **GamePurchased**: Update user libraries and analytics
- **GameUpdated**: Sync search indexes and caches

## 📚 API Documentation

### Swagger/OpenAPI
The service exposes comprehensive API documentation at:
- **Development**: `https://localhost:5002/swagger`
- **Staging**: `https://games-staging.tccloudgames.com/swagger`
- **Production**: `https://games.tccloudgames.com/swagger`

### Postman Collection
Import the provided Postman collection for easy API testing:
- **Collection**: `TC.CloudGames.Games.Api.postman_collection.json`
- **Environment**: `TC.CloudGames.Development.postman_environment.json`

## 🤝 Contributing

### Development Guidelines
1. **Architecture**: Follow hexagonal architecture principles
2. **Testing**: Maintain >80% code coverage
3. **Documentation**: Update API documentation with changes
4. **Performance**: Consider performance implications

### Code Standards
- **C#**: Follow Microsoft coding conventions
- **Validation**: Use FluentValidation for input validation
- **Logging**: Use structured logging with correlation IDs
- **Error Handling**: Implement comprehensive error handling

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

### Getting Help
- **Documentation**: Check API documentation and code comments
- **Issues**: Open an issue in the repository
- **Team**: Contact the Games team for specific questions

### Troubleshooting
- **Local Development**: Check Docker containers and dependencies
- **Search Issues**: Verify Elasticsearch connectivity and index status
- **Performance**: Use Application Insights for performance analysis

---

**TC Cloud Games - Games Service** - Powering the game catalog with modern microservices architecture.