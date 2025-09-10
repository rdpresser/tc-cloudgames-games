namespace TC.CloudGames.Games.Infrastructure.Repositories
{
    public class GameRepository : BaseRepository<GameAggregate>, IGameRepository
    {
        public GameRepository(IDocumentSession session)
            : base(session)
        {

        }

        public override Task<IEnumerable<GameAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a game by its unique identifier using GameProjection for optimized read operations.
        /// Only returns active games.
        /// </summary>
        /// <param name="id">The unique identifier of the game</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Game details if found and active, null otherwise</returns>
        public async Task<GameByIdResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var projection = await Session.Query<GameProjection>()
                .Where(g => g.IsActive && g.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.ReleaseDate,
                    x.AgeRating,
                    x.Description,
                    x.Developer,
                    x.Publisher,
                    x.DiskSizeInGb,
                    x.PriceAmount,
                    x.Genre,
                    x.PlatformListJson,
                    x.Tags,
                    x.GameMode,
                    x.DistributionFormat,
                    x.AvailableLanguages,
                    x.SupportsDlcs,
                    x.MinimumSystemRequirements,
                    x.RecommendedSystemRequirements,
                    x.PlaytimeHours,
                    x.PlayerCount,
                    x.RatingAverage,
                    x.OfficialLink,
                    x.GameStatus
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (projection == null)
                return null;

            return new GameByIdResponse
            {
                Id = projection.Id,
                Name = projection.Name,
                ReleaseDate = projection.ReleaseDate,
                AgeRating = projection.AgeRating,
                Description = projection.Description,
                DeveloperInfo = new DeveloperInfo(projection.Developer, projection.Publisher),
                DiskSize = projection.DiskSizeInGb,
                Price = projection.PriceAmount,
                Playtime = projection.PlaytimeHours.HasValue || projection.PlayerCount.HasValue
                    ? new Playtime(projection.PlaytimeHours, projection.PlayerCount)
                    : null,
                GameDetails = new GameDetails(
                    projection.Genre,
                    System.Text.Json.JsonSerializer.Deserialize<string[]>(projection.PlatformListJson) ?? Array.Empty<string>(),
                    projection.Tags,
                    projection.GameMode,
                    projection.DistributionFormat,
                    projection.AvailableLanguages,
                    projection.SupportsDlcs
                ),
                SystemRequirements = new SystemRequirements(projection.MinimumSystemRequirements, projection.RecommendedSystemRequirements),
                Rating = projection.RatingAverage,
                OfficialLink = projection.OfficialLink,
                GameStatus = projection.GameStatus
            };
        }

        /// <summary>
        /// Retrieves a paginated, filtered, and sorted list of games using GameProjection for optimized read operations.
        /// </summary>
        /// <param name="query">Query parameters for filtering, sorting, and pagination</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of games</returns>
        public async Task<IReadOnlyList<GameListResponse>> GetGameListAsync(GetGameListQuery query, CancellationToken cancellationToken = default)
        {
            // Start with active games
            var gamesQuery = Session.Query<GameProjection>()
                .Where(g => g.IsActive);

            // Dynamic filtering (search across multiple fields)
            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filter = query.Filter.ToLower();
                gamesQuery = gamesQuery.Where(g =>
                    g.Name.ToLower().Contains(filter) ||
                    g.Developer.ToLower().Contains(filter) ||
                    (g.Publisher != null && g.Publisher.ToLower().Contains(filter)) ||
                    (g.Genre != null && g.Genre.ToLower().Contains(filter)) ||
                    g.GameMode.ToLower().Contains(filter) ||
                    g.DistributionFormat.ToLower().Contains(filter) ||
                    (g.Tags != null && g.Tags.ToLower().Contains(filter)) ||
                    (g.GameStatus != null && g.GameStatus.ToLower().Contains(filter))
                );
            }

            // Dynamic sorting
            gamesQuery = query.SortBy.ToLower() switch
            {
                "name" => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.Name)
                    : gamesQuery.OrderBy(g => g.Name),
                "releasedate" => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.ReleaseDate)
                    : gamesQuery.OrderBy(g => g.ReleaseDate),
                "developer" => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.Developer)
                    : gamesQuery.OrderBy(g => g.Developer),
                "price" => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.PriceAmount)
                    : gamesQuery.OrderBy(g => g.PriceAmount),
                "rating" => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.RatingAverage)
                    : gamesQuery.OrderBy(g => g.RatingAverage),
                "agerating" => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.AgeRating)
                    : gamesQuery.OrderBy(g => g.AgeRating),
                "genre" => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.Genre)
                    : gamesQuery.OrderBy(g => g.Genre),
                "gamemode" => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.GameMode)
                    : gamesQuery.OrderBy(g => g.GameMode),
                "gamestatus" => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.GameStatus)
                    : gamesQuery.OrderBy(g => g.GameStatus),
                "createdat" => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.CreatedAt)
                    : gamesQuery.OrderBy(g => g.CreatedAt),
                _ => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.Id)
                    : gamesQuery.OrderBy(g => g.Id)
            };

            // Pagination
            gamesQuery = gamesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize);

            // Execute query and get projections
            var gameProjections = await gamesQuery
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Map to GameListResponse with proper JSON deserialization
            var gameList = gameProjections.Select(g => new GameListResponse
            {
                Id = g.Id,
                Name = g.Name,
                ReleaseDate = g.ReleaseDate,
                AgeRating = g.AgeRating,
                Description = g.Description,
                DeveloperInfo = new DeveloperInfo(g.Developer, g.Publisher),
                DiskSize = g.DiskSizeInGb,
                Price = g.PriceAmount,
                Playtime = g.PlaytimeHours.HasValue || g.PlayerCount.HasValue
                    ? new Playtime(g.PlaytimeHours, g.PlayerCount)
                    : null,
                GameDetails = new GameDetails(
                    g.Genre,
                    System.Text.Json.JsonSerializer.Deserialize<string[]>(g.PlatformListJson) ?? Array.Empty<string>(),
                    g.Tags,
                    g.GameMode,
                    g.DistributionFormat,
                    g.AvailableLanguages,
                    g.SupportsDlcs
                ),
                SystemRequirements = new SystemRequirements(g.MinimumSystemRequirements, g.RecommendedSystemRequirements),
                Rating = g.RatingAverage,
                OfficialLink = g.OfficialLink,
                GameStatus = g.GameStatus
            }).ToList();

            return gameList;
        }
    }
}
