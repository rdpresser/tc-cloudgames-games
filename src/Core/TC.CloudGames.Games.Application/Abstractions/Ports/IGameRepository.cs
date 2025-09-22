namespace TC.CloudGames.Games.Application.Abstractions.Ports
{
    public interface IGameRepository : IBaseRepository<GameAggregate>
    {
        Task<GameByIdResponse?> GetGameByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<GameListResponse>> GetGameListAsync(GetGameListQuery query, CancellationToken cancellationToken = default);
    }
}
