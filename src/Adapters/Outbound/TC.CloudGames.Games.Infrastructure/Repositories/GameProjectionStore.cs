using Marten.Linq;

namespace TC.CloudGames.Games.Infrastructure.Repositories
{
    public class GameProjectionStore : IGameProjectionStore
    {
        private readonly IDocumentSession _session;

        public GameProjectionStore(IDocumentSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public IMartenQueryable<GameProjection> Query()
        {
            return _session.Query<GameProjection>();
        }
    }
}
