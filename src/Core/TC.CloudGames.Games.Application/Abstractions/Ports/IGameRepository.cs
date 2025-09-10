using TC.CloudGames.Games.Application.UseCases.GetGameList;

namespace TC.CloudGames.Games.Application.Abstractions.Ports
{
    public interface IGameRepository : IBaseRepository<GameAggregate>
    {
        Task<GameByIdResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<GameListResponse>> GetGameListAsync(GetGameListQuery query, CancellationToken cancellationToken = default);
    }
}
