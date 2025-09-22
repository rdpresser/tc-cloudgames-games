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
                AgeRating = @event.AgeRating,
                Description = @event.Description,
                Developer = @event.Developer,
                Publisher = @event.Publisher,
                DiskSizeInGb = @event.DiskSizeInGb,
                PriceAmount = @event.PriceAmount,
                Genre = @event.Genre,
                Platforms = @event.Platforms.ToArray(),
                Tags = @event.Tags,
                GameMode = @event.GameMode,
                DistributionFormat = @event.DistributionFormat,
                AvailableLanguages = @event.AvailableLanguages,
                SupportsDlcs = @event.SupportsDlcs,
                MinimumSystemRequirements = @event.MinimumSystemRequirements,
                RecommendedSystemRequirements = @event.RecommendedSystemRequirements,
                PlaytimeHours = @event.PlaytimeHours,
                PlayerCount = @event.PlayerCount,
                RatingAverage = @event.RatingAverage,
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

            projection.PriceAmount = @event.PriceAmount;
            projection.UpdatedAt = @event.OccurredOn;

            operations.Store(projection);
        }

        public static async Task Project(GameStatusUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<GameProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.GameStatus = @event.GameStatus;
            projection.UpdatedAt = @event.OccurredOn;

            operations.Store(projection);
        }

        public static async Task Project(GameRatingUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<GameProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.RatingAverage = @event.RatingAverage;
            projection.UpdatedAt = @event.OccurredOn;

            operations.Store(projection);
        }

        public static async Task Project(GameDetailsUpdatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<GameProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.Genre = @event.Genre;
            projection.Platforms = @event.Platforms.ToArray();
            projection.Tags = @event.Tags;
            projection.GameMode = @event.GameMode;
            projection.DistributionFormat = @event.DistributionFormat;
            projection.AvailableLanguages = @event.AvailableLanguages;
            projection.SupportsDlcs = @event.SupportsDlcs;
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
