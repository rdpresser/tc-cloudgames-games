namespace TC.CloudGames.Games.Application.UseCases.CreateGame
{
    public static class CreateGameMapper
    {
        public static Result<GameAggregate> ToAggregate(CreateGameCommand command)
        {
            return CreateFromPrimitives(
                command.Name,
                command.ReleaseDate,
                command.AgeRating,
                command.DeveloperInfo.Developer,
                command.DeveloperInfo.Publisher,
                command.DiskSize,
                command.Price,
                command.GameDetails.Genre,
                command.GameDetails.Platform,
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
            return new CreateGameResponse(
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
                    [.. aggregate.GameDetails.PlatformList],
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

        public static GameCreatedIntegrationEvent ToIntegrationEvent(GameCreatedDomainEvent domainEvent)
        {
            return new GameCreatedIntegrationEvent(
                domainEvent.AggregateId,
                domainEvent.Name,
                domainEvent.ReleaseDate.ToDateTime(TimeOnly.MinValue),
                domainEvent.AgeRating.Value,
                domainEvent.Description,
                domainEvent.DeveloperInfo.Developer,
                domainEvent.DeveloperInfo.Publisher,
                domainEvent.DiskSize.SizeInGb,
                domainEvent.Price.Amount,
                domainEvent.GameDetails.Genre,
                [.. domainEvent.GameDetails.PlatformList],
                domainEvent.GameDetails.Tags,
                domainEvent.GameDetails.GameMode,
                domainEvent.GameDetails.DistributionFormat,
                domainEvent.GameDetails.AvailableLanguages,
                domainEvent.GameDetails.SupportsDlcs,
                domainEvent.SystemRequirements.Minimum,
                domainEvent.SystemRequirements.Recommended,
                domainEvent.Playtime?.Hours,
                domainEvent.Playtime?.PlayerCount,
                domainEvent.Rating?.Average,
                domainEvent.OfficialLink,
                domainEvent.GameStatus,
                domainEvent.OccurredOn
            );
        }
    }
}
