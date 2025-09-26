namespace TC.CloudGames.Games.Domain.Aggregates.Game
{
    /// <summary>
    /// Game aggregate root that manages game entities and their business rules using Event Sourcing pattern.
    /// </summary>
    public sealed class GameAggregate : BaseAggregateRoot
    {
        public string Name { get; private set; } = default!;
        public DateOnly ReleaseDate { get; private set; }
        public AgeRating AgeRating { get; private set; } = default!;
        public string? Description { get; private set; }
        public DeveloperInfo DeveloperInfo { get; private set; } = default!;
        public DiskSize DiskSize { get; private set; } = default!;
        public Price Price { get; private set; } = default!;
        public Playtime? Playtime { get; private set; }
        public GameDetails GameDetails { get; private set; } = default!;
        public SystemRequirements SystemRequirements { get; private set; } = default!;
        public Rating? Rating { get; private set; }
        public string? OfficialLink { get; private set; }
        public string? GameStatus { get; private set; }

        public static readonly IImmutableSet<string> ValidGameStatus = ImmutableHashSet.Create(
            "In Development", "Released", "Discontinued", "Available", "Soon", "Early Access"
        );

        // Construtor para Marten / ORM - deve ser público para event sourcing
        public GameAggregate() : base() { }

        // Construtor privado para factories
        private GameAggregate(Guid id) : base(id) { }

        #region Factories e Criação

        /// <summary>
        /// Creates a new GameAggregate using already validated Value Objects.
        /// </summary>
        public static Result<GameAggregate> Create(
            string name,
            DateOnly releaseDate,
            AgeRating ageRating,
            DeveloperInfo developerInfo,
            DiskSize diskSize,
            Price price,
            GameDetails gameDetails,
            SystemRequirements systemRequirements,
            string? description = null,
            Playtime? playtime = null,
            Rating? rating = null,
            string? officialLink = null,
            string? gameStatus = null)
        {
            var errors = new List<ValidationError>();
            if (!AgeRating.TryValidate(ageRating, out var ageRatingErrors)) errors.AddRange(ageRatingErrors);
            if (!DeveloperInfo.TryValidate(developerInfo, out var developerErrors)) errors.AddRange(developerErrors);
            if (!DiskSize.TryValidate(diskSize, out var diskSizeErrors)) errors.AddRange(diskSizeErrors);
            if (!Price.TryValidate(price, out var priceErrors)) errors.AddRange(priceErrors);
            if (!GameDetails.TryValidate(gameDetails, out var gameDetailsErrors)) errors.AddRange(gameDetailsErrors);
            if (!SystemRequirements.TryValidate(systemRequirements, out var systemReqErrors)) errors.AddRange(systemReqErrors);
            if (playtime != null && !Playtime.TryValidate(playtime, out var playtimeErrors)) errors.AddRange(playtimeErrors);
            if (rating != null && !Rating.TryValidate(rating, out var ratingErrors)) errors.AddRange(ratingErrors);
            errors.AddRange(ValidateGameProperties(name, releaseDate, description, officialLink, gameStatus));

            if (errors.Count > 0) return Result.Invalid(errors.ToArray());

            return CreateAggregate(name, releaseDate, ageRating, developerInfo, diskSize, price,
                gameDetails, systemRequirements, description, playtime, rating, officialLink, gameStatus);
        }

        /// <summary>
        /// Creates a new GameAggregate using Result<ValueObject> instances.
        /// </summary>
        public static Result<GameAggregate> CreateFromResult(
            string name,
            DateOnly releaseDate,
            Result<AgeRating> ageRating,
            Result<DeveloperInfo> developerInfo,
            Result<DiskSize> diskSize,
            Result<Price> price,
            Result<GameDetails> gameDetails,
            Result<SystemRequirements> systemRequirements,
            Result<Playtime>? playtime = null,
            Result<Rating>? rating = null,
            string? description = null,
            string? officialLink = null,
            string? gameStatus = null)
        {
            var errors = new List<ValidationError>();
            errors.AddErrorsIfFailure(ageRating);
            errors.AddErrorsIfFailure(developerInfo);
            errors.AddErrorsIfFailure(diskSize);
            errors.AddErrorsIfFailure(price);
            errors.AddErrorsIfFailure(gameDetails);
            errors.AddErrorsIfFailure(systemRequirements);

            if (playtime != null) errors.AddErrorsIfFailure(playtime);
            if (rating != null) errors.AddErrorsIfFailure(rating);

            errors.AddRange(ValidateGameProperties(name, releaseDate, description, officialLink, gameStatus));
            if (errors.Count > 0) return Result.Invalid(errors.ToArray());

            return CreateAggregate(name, releaseDate, ageRating.Value, developerInfo.Value, diskSize.Value, price.Value,
                gameDetails.Value, systemRequirements.Value, description, playtime?.Value, rating?.Value, officialLink, gameStatus);
        }

        /// <summary>
        /// Creates a new GameAggregate from primitive values with automatic validation.
        /// </summary>
        public static Result<GameAggregate> CreateFromPrimitives(
            string name,
            DateOnly releaseDate,
            string ageRating,
            string developer,
            string? publisher,
            decimal diskSizeInGb,
            decimal priceAmount,
            string? genre,
            IEnumerable<string> platforms,
            string? tags,
            string gameMode,
            string distributionFormat,
            string? availableLanguages,
            bool supportsDlcs,
            string minimumSystemRequirements,
            string? recommendedSystemRequirements = null,
            string? description = null,
            int? playtimeHours = null,
            int? playerCount = null,
            decimal? ratingAverage = null,
            string? officialLink = null,
            string? gameStatus = null)
        {
            var ageRatingResult = AgeRating.Create(ageRating);
            var developerInfoResult = DeveloperInfo.Create(developer, publisher);
            var diskSizeResult = DiskSize.Create(diskSizeInGb);
            var priceResult = Price.Create(priceAmount);
            var gameDetailsResult = GameDetails.Create(genre, platforms, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs);
            var systemRequirementsResult = SystemRequirements.Create(minimumSystemRequirements, recommendedSystemRequirements);

            Result<Playtime>? playtimeResult = null;
            Result<Rating>? ratingResult = null;

            if (playtimeHours.HasValue || playerCount.HasValue)
                playtimeResult = Playtime.Create(playtimeHours, playerCount);

            if (ratingAverage.HasValue)
                ratingResult = Rating.Create(ratingAverage);

            var errors = new List<ValidationError>();
            errors.AddErrorsIfFailure(ageRatingResult);
            errors.AddErrorsIfFailure(developerInfoResult);
            errors.AddErrorsIfFailure(diskSizeResult);
            errors.AddErrorsIfFailure(priceResult);
            errors.AddErrorsIfFailure(gameDetailsResult);
            errors.AddErrorsIfFailure(systemRequirementsResult);
            if (playtimeResult != null) errors.AddErrorsIfFailure(playtimeResult);
            if (ratingResult != null) errors.AddErrorsIfFailure(ratingResult);
            errors.AddRange(ValidateGameProperties(name, releaseDate, description, officialLink, gameStatus));

            if (errors.Count > 0) return Result.Invalid(errors.ToArray());

            return CreateAggregate(name, releaseDate, ageRatingResult.Value, developerInfoResult.Value, diskSizeResult.Value, priceResult.Value,
                gameDetailsResult.Value, systemRequirementsResult.Value, description, playtimeResult?.Value, ratingResult?.Value, officialLink, gameStatus);
        }

        private static Result<GameAggregate> CreateAggregate(
            string name,
            DateOnly releaseDate,
            AgeRating ageRating,
            DeveloperInfo developerInfo,
            DiskSize diskSize,
            Price price,
            GameDetails gameDetails,
            SystemRequirements systemRequirements,
            string? description,
            Playtime? playtime,
            Rating? rating,
            string? officialLink,
            string? gameStatus)
        {
            var aggregate = new GameAggregate(Guid.NewGuid());
            var @event = new GameCreatedDomainEvent(
                aggregate.Id,
                name,
                releaseDate,
                ageRating.Value,
                description,
                developerInfo.Developer,
                developerInfo.Publisher,
                diskSize.SizeInGb,
                price.Amount,
                gameDetails.Genre,
                gameDetails.Platforms.ToArray(),
                gameDetails.Tags,
                gameDetails.GameMode,
                gameDetails.DistributionFormat,
                gameDetails.AvailableLanguages,
                gameDetails.SupportsDlcs,
                systemRequirements.Minimum,
                systemRequirements.Recommended,
                playtime?.Hours,
                playtime?.PlayerCount,
                rating?.Average,
                officialLink,
                gameStatus,
                DateTimeOffset.UtcNow);
            aggregate.ApplyEvent(@event);
            return Result.Success(aggregate);
        }

        #endregion

        #region Update / Change Methods

        public Result UpdateBasicInfo(string name, string? description, string? officialLink)
        {
            var errors = ValidateBasicInfo(name, description, officialLink);
            if (errors.Any()) return Result.Invalid(errors.ToArray());

            var @event = new GameBasicInfoUpdatedDomainEvent(Id, name, description, officialLink, DateTimeOffset.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        public Result UpdatePrice(decimal newPriceAmount)
        {
            var priceResult = Price.Create(newPriceAmount);
            if (!priceResult.IsSuccess)
                return Result.Invalid(priceResult.ValidationErrors.ToArray());

            var @event = new GamePriceUpdatedDomainEvent(Id, priceResult.Value.Amount, DateTimeOffset.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        public Result UpdateGameStatus(string newStatus)
        {
            var errors = ValidateGameStatus(newStatus);
            if (errors.Any()) return Result.Invalid(errors.ToArray());

            if (GameStatus == newStatus)
                return Result.Invalid(new ValidationError("GameStatus.SameStatus", "Game already has this status."));

            var @event = new GameStatusUpdatedDomainEvent(Id, newStatus, DateTimeOffset.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        public Result UpdateRating(decimal? newRatingAverage)
        {
            Rating? newRating = null;
            if (newRatingAverage.HasValue)
            {
                var ratingResult = Rating.Create(newRatingAverage);
                if (!ratingResult.IsSuccess)
                    return Result.Invalid(ratingResult.ValidationErrors.ToArray());
                newRating = ratingResult.Value;
            }

            var @event = new GameRatingUpdatedDomainEvent(Id, newRating?.Average, DateTimeOffset.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        public Result UpdateGameDetails(
            string? genre,
            IEnumerable<string> platforms,
            string? tags,
            string gameMode,
            string distributionFormat,
            string? availableLanguages,
            bool supportsDlcs)
        {
            var gameDetailsResult = GameDetails.Create(genre, platforms, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs);
            if (!gameDetailsResult.IsSuccess)
                return Result.Invalid(gameDetailsResult.ValidationErrors.ToArray());

            var @event = new GameDetailsUpdatedDomainEvent(Id, genre, platforms.ToArray(), tags, gameMode, distributionFormat, availableLanguages, supportsDlcs, DateTimeOffset.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        public Result Activate()
        {
            if (IsActive) return Result.Invalid(new ValidationError("Game.AlreadyActive", "Game is already active."));
            var @event = new GameActivatedDomainEvent(Id, DateTimeOffset.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        public Result Deactivate()
        {
            if (!IsActive) return Result.Invalid(new ValidationError("Game.AlreadyInactive", "Game is already inactive."));
            var @event = new GameDeactivatedDomainEvent(Id, DateTimeOffset.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        #endregion

        #region Projection / ORM Factory

        public static GameAggregate FromProjection(
            Guid id,
            string name,
            DateOnly releaseDate,
            string ageRating,
            string? description,
            string developer,
            string? publisher,
            decimal diskSizeInGb,
            decimal priceAmount,
            string? genre,
            string[] platforms,
            string? tags,
            string gameMode,
            string distributionFormat,
            string? availableLanguages,
            bool supportsDlcs,
            string minimumSystemRequirements,
            string? recommendedSystemRequirements,
            int? playtimeHours,
            int? playerCount,
            decimal? ratingAverage,
            string? officialLink,
            string? gameStatus,
            DateTimeOffset createdAt,
            DateTimeOffset? updatedAt,
            bool isActive)
        {
            var game = new GameAggregate(id)
            {
                Name = name,
                ReleaseDate = releaseDate,
                AgeRating = AgeRating.FromDb(ageRating).Value,
                Description = description,
                DeveloperInfo = DeveloperInfo.FromDb(developer, publisher).Value,
                DiskSize = DiskSize.FromDb(diskSizeInGb).Value,
                Price = Price.FromDb(priceAmount).Value,
                GameDetails = GameDetails.FromDb(genre, platforms, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs).Value,
                SystemRequirements = SystemRequirements.FromDb(minimumSystemRequirements, recommendedSystemRequirements).Value,
                Playtime = (playtimeHours.HasValue || playerCount.HasValue) ? Playtime.FromDb(playtimeHours, playerCount).Value : null,
                Rating = ratingAverage.HasValue ? Rating.FromDb(ratingAverage).Value : null,
                OfficialLink = officialLink,
                GameStatus = gameStatus
            };

            game.SetActive(isActive);
            game.SetCreatedAt(createdAt);
            game.SetUpdatedAt(updatedAt);
            return game;
        }

        #endregion

        #region Domain Events Apply

        public void Apply(GameCreatedDomainEvent @event)
        {
            SetId(@event.AggregateId);
            Name = @event.Name;
            ReleaseDate = @event.ReleaseDate;
            AgeRating = AgeRating.FromDb(@event.AgeRating).Value;
            Description = @event.Description;
            DeveloperInfo = DeveloperInfo.FromDb(@event.Developer, @event.Publisher).Value;
            DiskSize = DiskSize.FromDb(@event.DiskSizeInGb).Value;
            Price = Price.FromDb(@event.PriceAmount).Value;
            GameDetails = GameDetails.FromDb(@event.Genre, @event.Platforms, @event.Tags, @event.GameMode, @event.DistributionFormat, @event.AvailableLanguages, @event.SupportsDlcs).Value;
            SystemRequirements = SystemRequirements.FromDb(@event.MinimumSystemRequirements, @event.RecommendedSystemRequirements).Value;
            Playtime = (@event.PlaytimeHours.HasValue || @event.PlayerCount.HasValue) ? Playtime.FromDb(@event.PlaytimeHours, @event.PlayerCount).Value : null;
            Rating = @event.RatingAverage.HasValue ? Rating.FromDb(@event.RatingAverage).Value : null;
            OfficialLink = @event.OfficialLink;
            GameStatus = @event.GameStatus;
            SetCreatedAt(@event.OccurredOn);
            SetActivate();
        }

        public void Apply(GameBasicInfoUpdatedDomainEvent @event)
        {
            Name = @event.Name;
            Description = @event.Description;
            OfficialLink = @event.OfficialLink;
            SetUpdatedAt(@event.OccurredOn);
        }

        public void Apply(GamePriceUpdatedDomainEvent @event)
        {
            Price = Price.FromDb(@event.PriceAmount).Value;
            SetUpdatedAt(@event.OccurredOn);
        }

        public void Apply(GameStatusUpdatedDomainEvent @event)
        {
            GameStatus = @event.GameStatus;
            SetUpdatedAt(@event.OccurredOn);
        }

        public void Apply(GameRatingUpdatedDomainEvent @event)
        {
            Rating = @event.RatingAverage.HasValue ? Rating.FromDb(@event.RatingAverage).Value : null;
            SetUpdatedAt(@event.OccurredOn);
        }

        public void Apply(GameDetailsUpdatedDomainEvent @event)
        {
            GameDetails = GameDetails.FromDb(@event.Genre, @event.Platforms, @event.Tags, @event.GameMode, @event.DistributionFormat, @event.AvailableLanguages, @event.SupportsDlcs).Value;
            SetUpdatedAt(@event.OccurredOn);
        }

        public void Apply(GameActivatedDomainEvent @event)
        {
            SetActivate();
            SetUpdatedAt(@event.OccurredOn);
        }

        public void Apply(GameDeactivatedDomainEvent @event)
        {
            SetDeactivate();
            SetUpdatedAt(@event.OccurredOn);
        }

        private void ApplyEvent(BaseDomainEvent @event)
        {
            AddNewEvent(@event);
            switch (@event)
            {
                case GameCreatedDomainEvent createdEvent: Apply(createdEvent); break;
                case GameBasicInfoUpdatedDomainEvent basicInfoEvent: Apply(basicInfoEvent); break;
                case GamePriceUpdatedDomainEvent priceEvent: Apply(priceEvent); break;
                case GameStatusUpdatedDomainEvent statusEvent: Apply(statusEvent); break;
                case GameRatingUpdatedDomainEvent ratingEvent: Apply(ratingEvent); break;
                case GameDetailsUpdatedDomainEvent detailsEvent: Apply(detailsEvent); break;
                case GameActivatedDomainEvent activatedEvent: Apply(activatedEvent); break;
                case GameDeactivatedDomainEvent deactivatedEvent: Apply(deactivatedEvent); break;
            }
        }

        #endregion

        #region Validation Helpers

        private static IEnumerable<ValidationError> ValidateGameProperties(
            string name, DateOnly releaseDate, string? description, string? officialLink, string? gameStatus)
        {
            foreach (var error in ValidateName(name))
                yield return error;
            foreach (var error in ValidateReleaseDate(releaseDate))
                yield return error;
            foreach (var error in ValidateDescription(description))
                yield return error;
            foreach (var error in ValidateOfficialLink(officialLink))
                yield return error;
            foreach (var error in ValidateGameStatus(gameStatus))
                yield return error;
        }

        private static IEnumerable<ValidationError> ValidateBasicInfo(string name, string? description, string? officialLink)
        {
            foreach (var error in ValidateName(name))
                yield return error;
            foreach (var error in ValidateDescription(description))
                yield return error;
            foreach (var error in ValidateOfficialLink(officialLink))
                yield return error;
        }

        private static IEnumerable<ValidationError> ValidateName(string name)
        {
            const int maxLength = 200;
            if (string.IsNullOrWhiteSpace(name))
                yield return new ValidationError("Name.Required", "Game name is required.");
            else if (name.Length > maxLength)
                yield return new ValidationError("Name.MaximumLength", $"Name cannot exceed {maxLength} characters.");
        }

        private static IEnumerable<ValidationError> ValidateReleaseDate(DateOnly releaseDate)
        {
            if (releaseDate == DateOnly.MinValue)
                yield return new ValidationError("ReleaseDate.Required", "Release date is required.");
            if (releaseDate <= DateOnly.MinValue)
                yield return new ValidationError("ReleaseDate.ValidDate", "Release date must be a valid date.");
        }

        private static IEnumerable<ValidationError> ValidateDescription(string? description)
        {
            const int maxLength = 2000;
            if (description != null && description.Length > maxLength)
                yield return new ValidationError("Description.MaximumLength", $"Description cannot exceed {maxLength} characters.");
        }

        private static IEnumerable<ValidationError> ValidateOfficialLink(string? officialLink)
        {
            const int maxLength = 200;
            if (!string.IsNullOrWhiteSpace(officialLink))
            {
                if (officialLink.Length > maxLength)
                    yield return new ValidationError("OfficialLink.MaximumLength", $"Official link must not exceed {maxLength} characters.");
                if (!Uri.IsWellFormedUriString(officialLink, UriKind.Absolute))
                    yield return new ValidationError("OfficialLink.ValidUrl", "Official link must be a valid URL.");
            }
        }

        private static IEnumerable<ValidationError> ValidateGameStatus(string? gameStatus)
        {
            const int maxLength = 200;
            if (gameStatus != null)
            {
                if (!ValidGameStatus.Contains(gameStatus))
                    yield return new ValidationError("GameStatus.ValidGameStatus", $"Invalid game status specified. Valid status are: {ValidGameStatus.JoinWithQuotes()}.");
                if (gameStatus.Length > maxLength)
                    yield return new ValidationError("GameStatus.MaximumLength", $"Game status must not exceed {maxLength} characters.");
            }
        }

        #endregion

        #region Domain Events (Primitive Values Only)

        public record GameCreatedDomainEvent(
            Guid AggregateId,
            string Name,
            DateOnly ReleaseDate,
            string AgeRating,
            string? Description,
            string Developer,
            string? Publisher,
            decimal DiskSizeInGb,
            decimal PriceAmount,
            string? Genre,
            string[] Platforms,
            string? Tags,
            string GameMode,
            string DistributionFormat,
            string? AvailableLanguages,
            bool SupportsDlcs,
            string MinimumSystemRequirements,
            string? RecommendedSystemRequirements,
            int? PlaytimeHours,
            int? PlayerCount,
            decimal? RatingAverage,
            string? OfficialLink,
            string? GameStatus,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameBasicInfoUpdatedDomainEvent(
            Guid AggregateId,
            string Name,
            string? Description,
            string? OfficialLink,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GamePriceUpdatedDomainEvent(
            Guid AggregateId,
            decimal PriceAmount,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameStatusUpdatedDomainEvent(
            Guid AggregateId,
            string GameStatus,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameRatingUpdatedDomainEvent(
            Guid AggregateId,
            decimal? RatingAverage,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameDetailsUpdatedDomainEvent(
            Guid AggregateId,
            string? Genre,
            string[] Platforms,
            string? Tags,
            string GameMode,
            string DistributionFormat,
            string? AvailableLanguages,
            bool SupportsDlcs,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameActivatedDomainEvent(
            Guid AggregateId,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameDeactivatedDomainEvent(
            Guid AggregateId,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        #endregion
    }
}
