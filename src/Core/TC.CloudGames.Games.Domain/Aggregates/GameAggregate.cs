using System.Collections.Immutable;
using TC.CloudGames.Games.Domain.ValueObjects;
using TC.CloudGames.SharedKernel.Domain.Aggregate;

namespace TC.CloudGames.Games.Domain.Aggregates
{
    public class GameAggregate : BaseAggregateRoot
    {
        public string Name { get; }
        public DateOnly ReleaseDate { get; }
        public AgeRating AgeRating { get; }
        public string? Description { get; }
        public DeveloperInfo DeveloperInfo { get; }
        public DiskSize DiskSize { get; }
        public Price Price { get; }
        public Playtime? Playtime { get; }
        public GameDetails GameDetails { get; }
        public SystemRequirements SystemRequirements { get; }
        public Rating? Rating { get; }
        public string? OfficialLink { get; }
        public string? GameStatus { get; }

        public static readonly IImmutableSet<string> ValidGameStatus = ImmutableHashSet.Create(
            "In Development", "Released", "Discontinued", "Available", "Soon", "Early Access"
        );

        private GameAggregate(Guid id)
            : base(id)
        {
        }
    }
}
