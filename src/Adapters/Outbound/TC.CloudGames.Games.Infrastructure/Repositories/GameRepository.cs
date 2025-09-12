namespace TC.CloudGames.Games.Infrastructure.Repositories
{
    public class GameRepository : BaseRepository<GameAggregate>, IGameRepository
    {
        public GameRepository(IDocumentSession session)
            : base(session)
        {
        }

        public override async Task<IEnumerable<GameAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var gameProjections = await Session.Query<GameProjection>()
                .Where(g => g.IsActive)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return gameProjections.Select(g =>
                GameAggregate.FromProjection(
                    g.Id,
                    g.Name,
                    g.ReleaseDate,
                    g.AgeRating,
                    g.Description,
                    g.Developer,
                    g.Publisher,
                    g.DiskSizeInGb,
                    g.PriceAmount,
                    g.Genre,
                    g.Platforms,
                    g.Tags,
                    g.GameMode,
                    g.DistributionFormat,
                    g.AvailableLanguages,
                    g.SupportsDlcs,
                    g.MinimumSystemRequirements,
                    g.RecommendedSystemRequirements,
                    g.PlaytimeHours,
                    g.PlayerCount,
                    g.RatingAverage,
                    g.OfficialLink,
                    g.GameStatus,
                    g.CreatedAt,
                    g.UpdatedAt,
                    g.IsActive
                ));
        }

        public async Task<GameByIdResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var projection = await Session.Query<GameProjection>()
                .Where(g => g.IsActive && g.Id == id)
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
                    projection.Platforms,
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

        public async Task<IReadOnlyList<GameListResponse>> GetGameListAsync(GetGameListQuery query, CancellationToken cancellationToken = default)
        {
            var gamesQuery = Session.Query<GameProjection>()
                .Where(g => g.IsActive);

            // Dynamic filtering
            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filter = query.Filter.ToLower();
                bool isGuid = Guid.TryParse(filter, out var guid);

                gamesQuery = gamesQuery.Where(g =>
                    (isGuid && g.Id == guid) ||
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
                _ => query.SortDirection.ToLower() == "desc"
                    ? gamesQuery.OrderByDescending(g => g.Id)
                    : gamesQuery.OrderBy(g => g.Id)
            };

            // Pagination
            gamesQuery = gamesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize);

            var gameProjections = await gamesQuery.ToListAsync(cancellationToken);

            return gameProjections.Select(g => new GameListResponse
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
                    g.Platforms,
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
        }
    }
}
