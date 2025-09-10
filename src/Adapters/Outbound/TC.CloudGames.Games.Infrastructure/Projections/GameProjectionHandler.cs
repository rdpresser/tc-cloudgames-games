using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marten.Events.Projections;
using static TC.CloudGames.Games.Domain.Aggregates.GameAggregate;

namespace TC.CloudGames.Games.Infrastructure.Projections
{
    /// <summary>
    /// Marten projection handler that builds GameProjection read models from domain events.
    /// This handler processes all game-related domain events and maintains the current state
    /// of game projections for optimized read operations.
    /// </summary>
    public class GameProjectionHandler : EventProjection
    {
        public static void Project(GameCreatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = new GameProjection
            {
                Id = @event.AggregateId,
                Name = @event.Name,
                ReleaseDate = @event.ReleaseDate,
                AgeRating = @event.AgeRating.Value,
                Description = @event.Description,
                Developer = @event.DeveloperInfo.Developer,
                Publisher = @event.DeveloperInfo.Publisher,
                DiskSizeInGb = @event.DiskSize.SizeInGb,
                PriceAmount = @event.Price.Amount,
                Genre = @event.GameDetails.Genre,
                PlatformListJson = @event.GameDetails.GetPlatformListAsJson(),
                Tags = @event.GameDetails.Tags,
                GameMode = @event.GameDetails.GameMode,
                DistributionFormat = @event.GameDetails.DistributionFormat,
                AvailableLanguages = @event.GameDetails.AvailableLanguages,
                SupportsDlcs = @event.GameDetails.SupportsDlcs,
                MinimumSystemRequirements = @event.SystemRequirements.Minimum,
                RecommendedSystemRequirements = @event.SystemRequirements.Recommended,
                PlaytimeHours = @event.Playtime?.Hours,
                PlayerCount = @event.Playtime?.PlayerCount,
                RatingAverage = @event.Rating?.Average,
                OfficialLink = @event.OfficialLink,
                GameStatus = @event.GameStatus,
                CreatedAt = @event.OccurredOn,
                IsActive = true
            };
            operations.Store(projection);
        }

        public static void Project(GameBasicInfoUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = new GameProjection
            {
                Id = @event.AggregateId,
                Name = @event.Name,
                Description = @event.Description,
                OfficialLink = @event.OfficialLink,
                UpdatedAt = @event.OccurredOn,
                IsActive = true // Assume still active unless deactivated event is processed
            };
            operations.Store(projection);
        }

        public static void Project(GamePriceUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = new GameProjection
            {
                Id = @event.AggregateId,
                PriceAmount = @event.NewPrice.Amount,
                UpdatedAt = @event.OccurredOn,
                IsActive = true // Assume still active unless deactivated event is processed
            };
            operations.Store(projection);
        }

        public static void Project(GameStatusUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = new GameProjection
            {
                Id = @event.AggregateId,
                GameStatus = @event.NewStatus,
                UpdatedAt = @event.OccurredOn,
                IsActive = true // Assume still active unless deactivated event is processed
            };
            operations.Store(projection);
        }

        public static void Project(GameRatingUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = new GameProjection
            {
                Id = @event.AggregateId,
                RatingAverage = @event.NewRating?.Average,
                UpdatedAt = @event.OccurredOn,
                IsActive = true // Assume still active unless deactivated event is processed
            };
            operations.Store(projection);
        }

        public static void Project(GameDetailsUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = new GameProjection
            {
                Id = @event.AggregateId,
                Genre = @event.NewGameDetails.Genre,
                PlatformListJson = @event.NewGameDetails.GetPlatformListAsJson(),
                Tags = @event.NewGameDetails.Tags,
                GameMode = @event.NewGameDetails.GameMode,
                DistributionFormat = @event.NewGameDetails.DistributionFormat,
                AvailableLanguages = @event.NewGameDetails.AvailableLanguages,
                SupportsDlcs = @event.NewGameDetails.SupportsDlcs,
                UpdatedAt = @event.OccurredOn,
                IsActive = true // Assume still active unless deactivated event is processed
            };
            operations.Store(projection);
        }

        public static void Project(GameActivatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = new GameProjection
            {
                Id = @event.AggregateId,
                IsActive = true,
                UpdatedAt = @event.OccurredOn
            };
            operations.Store(projection);
        }

        public static void Project(GameDeactivatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = new GameProjection
            {
                Id = @event.AggregateId,
                IsActive = false,
                UpdatedAt = @event.OccurredOn
            };
            operations.Store(projection);
        }
    }
}
