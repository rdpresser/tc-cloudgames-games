namespace TC.CloudGames.Games.Infrastructure.Projections
{
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
                Platforms = @event.GameDetails.Platforms.ToArray(),
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
                UpdatedAt = null,
                IsActive = true
            };

            operations.Store(projection);
        }

        public static async Task Project(GameBasicInfoUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<GameProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.Name = @event.Name;
            projection.Description = @event.Description;
            projection.OfficialLink = @event.OfficialLink;
            projection.UpdatedAt = @event.OccurredOn;

            operations.Store(projection);
        }

        public static async Task Project(GamePriceUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<GameProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.PriceAmount = @event.NewPrice.Amount;
            projection.UpdatedAt = @event.OccurredOn;

            operations.Store(projection);
        }

        public static async Task Project(GameStatusUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<GameProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.GameStatus = @event.NewStatus;
            projection.UpdatedAt = @event.OccurredOn;

            operations.Store(projection);
        }

        public static async Task Project(GameRatingUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<GameProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.RatingAverage = @event.NewRating?.Average;
            projection.UpdatedAt = @event.OccurredOn;

            operations.Store(projection);
        }

        public static async Task Project(GameDetailsUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<GameProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.Genre = @event.NewGameDetails.Genre;
            projection.Platforms = @event.NewGameDetails.Platforms.ToArray();
            projection.Tags = @event.NewGameDetails.Tags;
            projection.GameMode = @event.NewGameDetails.GameMode;
            projection.DistributionFormat = @event.NewGameDetails.DistributionFormat;
            projection.AvailableLanguages = @event.NewGameDetails.AvailableLanguages;
            projection.SupportsDlcs = @event.NewGameDetails.SupportsDlcs;
            projection.UpdatedAt = @event.OccurredOn;

            operations.Store(projection);
        }

        public static async Task Project(GameActivatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<GameProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.IsActive = true;
            projection.UpdatedAt = @event.OccurredOn;

            operations.Store(projection);
        }

        public static async Task Project(GameDeactivatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<GameProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.IsActive = false;
            projection.UpdatedAt = @event.OccurredOn;

            operations.Store(projection);
        }
    }
}
