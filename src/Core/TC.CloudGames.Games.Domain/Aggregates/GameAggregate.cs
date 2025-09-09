using TC.CloudGames.SharedKernel.Domain.Aggregate;

namespace TC.CloudGames.Games.Domain.Aggregates
{
    public class GameAggregate : BaseAggregateRoot
    {
        private GameAggregate(Guid id)
            : base(id)
        {
        }
    }
}
