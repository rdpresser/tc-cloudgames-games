using Marten.Linq;

namespace TC.CloudGames.Games.Infrastructure.Repositories
{
    public interface IGameProjectionStore
    {
        IMartenQueryable<GameProjection> Query();
    }
}
