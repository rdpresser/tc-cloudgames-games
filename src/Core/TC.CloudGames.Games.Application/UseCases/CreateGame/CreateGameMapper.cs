using TC.CloudGames.Games.Application.Abstractions.Projections;

namespace TC.CloudGames.Games.Application.UseCases.CreateGame
{
    public static class CreateGameMapper
    {
        public static Result<GameAggregate> ToAggregate(CreateGameCommand command)
        {
            return GameAggregate.CreateFromPrimitives(
                command.Name,
                command.ReleaseDate,
                command.AgeRating,
                command.DeveloperInfo.Developer,
                command.DeveloperInfo.Publisher,
                command.DiskSize,
                command.Price,
                command.GameDetails.Genre,
                command.GameDetails.Platforms,
                command.GameDetails.Tags,
                command.GameDetails.GameMode,
                command.GameDetails.DistributionFormat,
                command.GameDetails.AvailableLanguages,
                command.GameDetails.SupportsDlcs,
                command.SystemRequirements.Minimum,
                command.SystemRequirements.Recommended,
                command.Description,
                command.Playtime?.Hours,
                command.Playtime?.PlayerCount,
                command.Rating,
                command.OfficialLink,
                command.GameStatus
            );
        }

        public static CreateGameResponse FromAggregate(GameAggregate aggregate)
        {
            return new(
                aggregate.Id,
                aggregate.Name,
                aggregate.ReleaseDate,
                aggregate.AgeRating.Value,
                aggregate.Description,
                new DeveloperInfo(aggregate.DeveloperInfo.Developer, aggregate.DeveloperInfo.Publisher),
                aggregate.DiskSize.SizeInGb,
                aggregate.Price.Amount,
                aggregate.Playtime is not null
                    ? new Playtime(aggregate.Playtime.Hours, aggregate.Playtime.PlayerCount)
                    : null,
                new GameDetails(
                    aggregate.GameDetails.Genre,
                    [.. aggregate.GameDetails.Platforms],
                    aggregate.GameDetails.Tags,
                    aggregate.GameDetails.GameMode,
                    aggregate.GameDetails.DistributionFormat,
                    aggregate.GameDetails.AvailableLanguages,
                    aggregate.GameDetails.SupportsDlcs
                ),
                new SystemRequirements(aggregate.SystemRequirements.Minimum, aggregate.SystemRequirements.Recommended),
                aggregate.Rating?.Average,
                aggregate.OfficialLink,
                aggregate.GameStatus
            );
        }

        public static GameCreatedIntegrationEvent ToIntegrationEvent(GameAggregate.GameCreatedDomainEvent domainEvent)
        {
            return new(
                domainEvent.AggregateId,
                domainEvent.Name,
                domainEvent.ReleaseDate.ToDateTime(TimeOnly.MinValue),
                domainEvent.AgeRating,
                domainEvent.Description,
                domainEvent.Developer,
                domainEvent.Publisher,
                domainEvent.DiskSizeInGb,
                domainEvent.PriceAmount,
                domainEvent.Genre,
                [.. domainEvent.Platforms],
                domainEvent.Tags,
                domainEvent.GameMode,
                domainEvent.DistributionFormat,
                domainEvent.AvailableLanguages,
                domainEvent.SupportsDlcs,
                domainEvent.MinimumSystemRequirements,
                domainEvent.RecommendedSystemRequirements,
                domainEvent.PlaytimeHours,
                domainEvent.PlayerCount,
                domainEvent.RatingAverage,
                domainEvent.OfficialLink,
                domainEvent.GameStatus,
                domainEvent.OccurredOn
            );
        }

        public static GameProjection MapToProjection(CreateGameResponse response)
        {
            return new()
            {
                Id = response.Id,
                Name = response.Name,
                Description = response.Description,
                ReleaseDate = response.ReleaseDate,
                AgeRating = response.AgeRating,
                Developer = response.DeveloperInfo.Developer,
                Publisher = response.DeveloperInfo.Publisher,
                PriceAmount = response.Price,
                RatingAverage = response.Rating,
                Genre = response.GameDetails.Genre,
                Platforms = response.GameDetails.Platforms,
                Tags = response.GameDetails.Tags,
                GameMode = response.GameDetails.GameMode,
                DistributionFormat = response.GameDetails.DistributionFormat,
                AvailableLanguages = response.GameDetails.AvailableLanguages,
                SupportsDlcs = response.GameDetails.SupportsDlcs,
                GameStatus = response.GameStatus
            };
        }
    }
}
