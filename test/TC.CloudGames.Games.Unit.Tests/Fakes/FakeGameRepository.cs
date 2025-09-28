using TC.CloudGames.Games.Application.Abstractions.Ports;
using TC.CloudGames.Games.Application.UseCases.GetGameById;
using TC.CloudGames.Games.Application.UseCases.GetGameList;
using TC.CloudGames.Games.Domain.Aggregates.Game;

namespace TC.CloudGames.Games.Unit.Tests.Fakes
{
    /// <summary>
    /// Fake implementation of IGameRepository for testing purposes.
    /// Maintains an in-memory list of games to simulate repository behavior.
    /// </summary>
    public class FakeGameRepository : IGameRepository
    {
        private readonly List<GameAggregate> _games = new();

        #region IBaseRepository<GameAggregate> Implementations

        public Task<GameAggregate?> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default)
            => Task.FromResult(_games.FirstOrDefault(g => g.Id == aggregateId));

        public Task<GameAggregate> LoadAsync(Guid aggregateId, CancellationToken cancellationToken = default)
            => Task.FromResult(_games.First(g => g.Id == aggregateId));

        public Task SaveAsync(GameAggregate aggregate, CancellationToken cancellationToken = default)
        {
            var existing = _games.FirstOrDefault(g => g.Id == aggregate.Id);
            if (existing != null)
            {
                _games.Remove(existing);
            }
            _games.Add(aggregate);
            return Task.CompletedTask;
        }

        public Task PersistAsync(GameAggregate aggregate, CancellationToken cancellationToken = default)
            => SaveAsync(aggregate, cancellationToken);

        public Task CommitAsync(GameAggregate aggregate, CancellationToken cancellationToken = default)
            => Task.CompletedTask; // In-memory, nothing extra to commit

        public Task<IEnumerable<GameAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<GameAggregate>>(_games);

        public Task DeleteAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            _games.RemoveAll(g => g.Id == aggregateId);
            return Task.CompletedTask;
        }

        public Task<GameAggregate?> LoadSnapshotAsync(Guid aggregateId, CancellationToken cancellationToken = default)
            => GetByIdAsync(aggregateId, cancellationToken);

        public Task<GameByIdResponse?> GetByIdAsync(GetGameByIdQuery query, CancellationToken cancellationToken = default)
        {
            var game = _games.FirstOrDefault(g => g.Id == query.GameId);
            if (game == null) return Task.FromResult<GameByIdResponse?>(null);

            return Task.FromResult<GameByIdResponse?>(
                new GameByIdResponse
                {
                    Id = game.Id,
                    Name = game.Name,
                    ReleaseDate = game.ReleaseDate,
                    AgeRating = game.AgeRating.Value,
                    Description = game.Description,
                    DeveloperInfo = new DeveloperInfo(game.DeveloperInfo.Developer, game.DeveloperInfo.Publisher),
                    DiskSize = game.DiskSize.SizeInGb,
                    Price = game.Price.Amount,
                    Playtime = game.Playtime,
                    GameDetails = new GameDetails(
                        game.GameDetails.Genre,
                        game.GameDetails.Platforms.ToArray(),
                        game.GameDetails.Tags,
                        game.GameDetails.GameMode,
                        game.GameDetails.DistributionFormat,
                        game.GameDetails.AvailableLanguages,
                        game.GameDetails.SupportsDlcs
                    ),
                    SystemRequirements = new SystemRequirements(
                        game.SystemRequirements.Minimum,
                        game.SystemRequirements.Recommended
                    ),
                    Rating = game.Rating?.Average,
                    OfficialLink = game.OfficialLink,
                    GameStatus = game.GameStatus
                }
            );
        }

        public Task<GameByIdResponse?> GetGameByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var game = _games.FirstOrDefault(g => g.Id == id);
            if (game == null) return Task.FromResult<GameByIdResponse?>(null);

            return Task.FromResult<GameByIdResponse?>(
                new GameByIdResponse
                {
                    Id = game.Id,
                    Name = game.Name,
                    ReleaseDate = game.ReleaseDate,
                    AgeRating = game.AgeRating.Value,
                    Description = game.Description,
                    DeveloperInfo = new DeveloperInfo(game.DeveloperInfo.Developer, game.DeveloperInfo.Publisher),
                    DiskSize = game.DiskSize.SizeInGb,
                    Price = game.Price.Amount,
                    Playtime = game.Playtime,
                    GameDetails = new GameDetails(
                        game.GameDetails.Genre,
                        game.GameDetails.Platforms.ToArray(),
                        game.GameDetails.Tags,
                        game.GameDetails.GameMode,
                        game.GameDetails.DistributionFormat,
                        game.GameDetails.AvailableLanguages,
                        game.GameDetails.SupportsDlcs
                    ),
                    SystemRequirements = new SystemRequirements(
                        game.SystemRequirements.Minimum,
                        game.SystemRequirements.Recommended
                    ),
                    Rating = game.Rating?.Average,
                    OfficialLink = game.OfficialLink,
                    GameStatus = game.GameStatus
                }
            );
        }

        public Task<IReadOnlyList<GameListResponse>> GetGameListAsync(GetGameListQuery query, CancellationToken cancellationToken = default)
        {
            var list = _games
                .Where(g => g.IsActive)
                .Select(g => new GameListResponse 
                { 
                    Id = g.Id, 
                    Name = g.Name, 
                    ReleaseDate = g.ReleaseDate,
                    AgeRating = g.AgeRating.Value,
                    Description = g.Description,
                    DeveloperInfo = new DeveloperInfo(g.DeveloperInfo.Developer, g.DeveloperInfo.Publisher),
                    DiskSize = g.DiskSize.SizeInGb,
                    Price = g.Price.Amount,
                    Playtime = g.Playtime,
                    GameDetails = new GameDetails(
                        g.GameDetails.Genre,
                        g.GameDetails.Platforms.ToArray(),
                        g.GameDetails.Tags,
                        g.GameDetails.GameMode,
                        g.GameDetails.DistributionFormat,
                        g.GameDetails.AvailableLanguages,
                        g.GameDetails.SupportsDlcs
                    ),
                    SystemRequirements = new SystemRequirements(
                        g.SystemRequirements.Minimum,
                        g.SystemRequirements.Recommended
                    ),
                    Rating = g.Rating?.Average,
                    OfficialLink = g.OfficialLink,
                    GameStatus = g.GameStatus
                })
                .ToList()
                .AsReadOnly();

            return Task.FromResult<IReadOnlyList<GameListResponse>>(list);
        }

        #endregion

        #region Helpers for Tests

        /// <summary>
        /// Adds a game directly to the fake repository (for testing purposes).
        /// </summary>
        public void AddFakeGame(GameAggregate game)
        {
            _games.Add(game);
        }

        /// <summary>
        /// Clears all games from the repository.
        /// </summary>
        public void Clear()
        {
            _games.Clear();
        }

        /// <summary>
        /// Gets the count of games in the repository.
        /// </summary>
        public int Count => _games.Count;

        #endregion
    }
}
