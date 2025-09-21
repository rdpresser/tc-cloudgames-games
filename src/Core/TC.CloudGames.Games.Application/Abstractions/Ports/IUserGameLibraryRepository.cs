using TC.CloudGames.Games.Domain.Aggregates.UserGameLibrary;

namespace TC.CloudGames.Games.Application.Abstractions.Ports
{
    public interface IUserGameLibraryRepository : IBaseRepository<UserGameLibraryAggregate>
    {
        Task<bool> UserOwnsGameAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
    }
}
