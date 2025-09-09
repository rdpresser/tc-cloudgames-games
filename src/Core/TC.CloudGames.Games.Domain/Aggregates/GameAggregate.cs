using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Ardalis.Result;
using TC.CloudGames.Games.Domain.ValueObjects;
using TC.CloudGames.SharedKernel.Domain.Aggregate;
using TC.CloudGames.SharedKernel.Domain.Events;
using TC.CloudGames.SharedKernel.Extensions;

namespace TC.CloudGames.Games.Domain.Aggregates
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

        // Private constructor for aggregate reconstruction
        private GameAggregate(Guid id) : base(id) { }

        #region Factory Methods

        /// <summary>
        /// Creates a new GameAggregate with proper validation using Value Objects and primitive values.
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

            if (!AgeRating.TryValidate(ageRating, out var ageRatingErrors))
                errors.AddRange(ageRatingErrors);
            if (!DeveloperInfo.TryValidate(developerInfo, out var developerErrors))
                errors.AddRange(developerErrors);
            if (!DiskSize.TryValidate(diskSize, out var diskSizeErrors))
                errors.AddRange(diskSizeErrors);
            if (!Price.TryValidate(price, out var priceErrors))
                errors.AddRange(priceErrors);
            if (!GameDetails.TryValidate(gameDetails, out var gameDetailsErrors))
                errors.AddRange(gameDetailsErrors);
            if (!SystemRequirements.TryValidate(systemRequirements, out var systemReqErrors))
                errors.AddRange(systemReqErrors);
            if (playtime != null && !Playtime.TryValidate(playtime, out var playtimeErrors))
                errors.AddRange(playtimeErrors);
            if (rating != null && !Rating.TryValidate(rating, out var ratingErrors))
                errors.AddRange(ratingErrors);

            errors.AddRange(ValidateGameProperties(name, releaseDate, description, officialLink, gameStatus));
            if (errors.Count > 0)
                return Result.Invalid(errors.ToArray());

            return CreateAggregate(name, releaseDate, ageRating, developerInfo, diskSize, price, 
                gameDetails, systemRequirements, description, playtime, rating, officialLink, gameStatus);
        }

        /// <summary>
        /// Creates a new GameAggregate from primitive values with automatic validation.
        /// </summary>
        public static Result<GameAggregate> CreateFromPrimitives(
            string name,
            DateOnly releaseDate,
            string ageRatingValue,
            string developer,
            string? publisher,
            decimal diskSizeInGb,
            decimal priceAmount,
            string? genre,
            IReadOnlyCollection<string> platformList,
            string? tags,
            string gameMode,
            string distributionFormat,
            string? availableLanguages,
            bool supportsDlcs,
            string minimumSystemReq,
            string? recommendedSystemReq = null,
            string? description = null,
            int? playtimeHours = null,
            int? playerCount = null,
            decimal? ratingAverage = null,
            string? officialLink = null,
            string? gameStatus = null)
        {
            var ageRatingResult = AgeRating.Create(ageRatingValue);
            var developerInfoResult = DeveloperInfo.Create(developer, publisher);
            var diskSizeResult = DiskSize.Create(diskSizeInGb);
            var priceResult = Price.Create(priceAmount);
            var gameDetailsResult = GameDetails.Create(genre, platformList, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs);
            var systemReqResult = SystemRequirements.Create(minimumSystemReq, recommendedSystemReq);
            
            Playtime? playtime = null;
            Rating? rating = null;

            var errors = new List<ValidationError>();
            AddErrorsIfFailure(errors, ageRatingResult);
            AddErrorsIfFailure(errors, developerInfoResult);
            AddErrorsIfFailure(errors, diskSizeResult);
            AddErrorsIfFailure(errors, priceResult);
            AddErrorsIfFailure(errors, gameDetailsResult);
            AddErrorsIfFailure(errors, systemReqResult);

            if (playtimeHours.HasValue || playerCount.HasValue)
            {
                var playtimeResult = Playtime.Create(playtimeHours, playerCount);
                AddErrorsIfFailure(errors, playtimeResult);
                if (playtimeResult.IsSuccess)
                    playtime = playtimeResult.Value;
            }

            if (ratingAverage.HasValue)
            {
                var ratingResult = Rating.Create(ratingAverage);
                AddErrorsIfFailure(errors, ratingResult);
                if (ratingResult.IsSuccess)
                    rating = ratingResult.Value;
            }

            errors.AddRange(ValidateGameProperties(name, releaseDate, description, officialLink, gameStatus));

            if (errors.Count > 0)
                return Result.Invalid(errors.ToArray());

            return CreateAggregate(name, releaseDate, ageRatingResult.Value, developerInfoResult.Value, 
                diskSizeResult.Value, priceResult.Value, gameDetailsResult.Value, systemReqResult.Value,
                description, playtime, rating, officialLink, gameStatus);
        }

        /// <summary>
        /// Factory method to reconstruct aggregate from projection data (for read operations).
        /// </summary>
        public static GameAggregate FromProjection(
            Guid id,
            string name,
            DateOnly releaseDate,
            string ageRatingValue,
            string? description,
            string developer,
            string? publisher,
            decimal diskSizeInGb,
            decimal priceAmount,
            string? genre,
            string platformListJson,
            string? tags,
            string gameMode,
            string distributionFormat,
            string? availableLanguages,
            bool supportsDlcs,
            string minimumSystemReq,
            string? recommendedSystemReq,
            int? playtimeHours,
            int? playerCount,
            decimal? ratingAverage,
            string? officialLink,
            string? gameStatus,
            DateTime createdAt,
            DateTime? updatedAt,
            bool isActive)
        {
            var game = new GameAggregate(id)
            {
                Name = name,
                ReleaseDate = releaseDate,
                AgeRating = AgeRating.FromDb(ageRatingValue).Value,
                Description = description,
                DeveloperInfo = DeveloperInfo.FromDb(developer, publisher).Value,
                DiskSize = DiskSize.FromDb(diskSizeInGb).Value,
                Price = Price.FromDb(priceAmount).Value,
                GameDetails = GameDetails.FromDb(genre, platformListJson, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs).Value,
                SystemRequirements = SystemRequirements.FromDb(minimumSystemReq, recommendedSystemReq).Value,
                Playtime = playtimeHours.HasValue || playerCount.HasValue ? Playtime.FromDb(playtimeHours, playerCount).Value : null,
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

        #region Business Operations

        /// <summary>
        /// Updates game basic information with validation.
        /// </summary>
        public Result UpdateBasicInfo(string name, string? description, string? officialLink)
        {
            var errors = ValidateBasicInfo(name, description, officialLink);
            if (errors.Any())
                return Result.Invalid(errors.ToArray());

            var @event = new GameBasicInfoUpdatedDomainEvent(Id, name, description, officialLink, DateTime.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        /// <summary>
        /// Updates game pricing information.
        /// </summary>
        public Result UpdatePrice(decimal newPriceAmount)
        {
            var priceResult = Price.Create(newPriceAmount);
            if (!priceResult.IsSuccess)
                return Result.Invalid(priceResult.ValidationErrors.ToArray());

            var @event = new GamePriceUpdatedDomainEvent(Id, priceResult.Value, DateTime.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        /// <summary>
        /// Updates game status with validation.
        /// </summary>
        public Result UpdateGameStatus(string newStatus)
        {
            var errors = ValidateGameStatus(newStatus);
            if (errors.Any())
                return Result.Invalid(errors.ToArray());

            if (GameStatus == newStatus)
                return Result.Invalid(new ValidationError("GameStatus.SameStatus", "Game already has this status."));

            var @event = new GameStatusUpdatedDomainEvent(Id, newStatus, DateTime.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        /// <summary>
        /// Updates game rating.
        /// </summary>
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

            var @event = new GameRatingUpdatedDomainEvent(Id, newRating, DateTime.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        /// <summary>
        /// Updates game details information.
        /// </summary>
        public Result UpdateGameDetails(
            string? genre,
            IReadOnlyCollection<string> platformList,
            string? tags,
            string gameMode,
            string distributionFormat,
            string? availableLanguages,
            bool supportsDlcs)
        {
            var gameDetailsResult = GameDetails.Create(genre, platformList, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs);
            if (!gameDetailsResult.IsSuccess)
                return Result.Invalid(gameDetailsResult.ValidationErrors.ToArray());

            var @event = new GameDetailsUpdatedDomainEvent(Id, gameDetailsResult.Value, DateTime.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        /// <summary>
        /// Activates a deactivated game.
        /// </summary>
        public Result Activate()
        {
            if (IsActive)
                return Result.Invalid(new ValidationError("Game.AlreadyActive", "Game is already active."));

            var @event = new GameActivatedDomainEvent(Id, DateTime.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        /// <summary>
        /// Deactivates an active game.
        /// </summary>
        public Result Deactivate()
        {
            if (!IsActive)
                return Result.Invalid(new ValidationError("Game.AlreadyInactive", "Game is already deactivated."));

            var @event = new GameDeactivatedDomainEvent(Id, DateTime.UtcNow);
            ApplyEvent(@event);
            return Result.Success();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Helper method to add errors from failed results to the error list.
        /// </summary>
        private static void AddErrorsIfFailure<T>(List<ValidationError> errors, Result<T> result)
        {
            if (!result.IsSuccess)
                errors.AddRange(result.ValidationErrors);
        }

        /// <summary>
        /// Creates and returns a new GameAggregate instance and applies the GameCreatedEvent.
        /// </summary>
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
                aggregate.Id, name, releaseDate, ageRating, description, developerInfo,
                diskSize, price, playtime, gameDetails, systemRequirements, rating,
                officialLink, gameStatus, aggregate.CreatedAt);
            aggregate.ApplyEvent(@event);
            return Result.Success(aggregate);
        }

        /// <summary>
        /// Validates game-specific properties and returns validation errors.
        /// </summary>
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

        /// <summary>
        /// Validates basic info properties.
        /// </summary>
        private static IEnumerable<ValidationError> ValidateBasicInfo(string name, string? description, string? officialLink)
        {
            foreach (var error in ValidateName(name))
                yield return error;
            foreach (var error in ValidateDescription(description))
                yield return error;
            foreach (var error in ValidateOfficialLink(officialLink))
                yield return error;
        }

        /// <summary>
        /// Validates game name.
        /// </summary>
        private static IEnumerable<ValidationError> ValidateName(string name)
        {
            const int maxLength = 200;
            if (string.IsNullOrWhiteSpace(name))
                yield return new ValidationError("Name.Required", "Game name is required.");
            else if (name.Length > maxLength)
                yield return new ValidationError("Name.MaximumLength", $"Name cannot exceed {maxLength} characters.");
        }

        /// <summary>
        /// Validates release date.
        /// </summary>
        private static IEnumerable<ValidationError> ValidateReleaseDate(DateOnly releaseDate)
        {
            if (releaseDate == DateOnly.MinValue)
                yield return new ValidationError("ReleaseDate.Required", "Release date is required.");
            if (releaseDate <= DateOnly.MinValue)
                yield return new ValidationError("ReleaseDate.ValidDate", "Release date must be a valid date.");
        }

        /// <summary>
        /// Validates description.
        /// </summary>
        private static IEnumerable<ValidationError> ValidateDescription(string? description)
        {
            const int maxLength = 2000;
            if (description != null && description.Length > maxLength)
                yield return new ValidationError("Description.MaximumLength", $"Description cannot exceed {maxLength} characters.");
        }

        /// <summary>
        /// Validates official link.
        /// </summary>
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

        /// <summary>
        /// Validates game status.
        /// </summary>
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

        #region Event Application

        /// <summary>
        /// Applies an event to the aggregate and adds it to uncommitted events.
        /// </summary>
        private void ApplyEvent(BaseDomainEvent @event)
        {
            AddNewEvent(@event);
            switch (@event)
            {
                case GameCreatedDomainEvent createdEvent:
                    Apply(createdEvent);
                    break;
                case GameBasicInfoUpdatedDomainEvent basicInfoEvent:
                    Apply(basicInfoEvent);
                    break;
                case GamePriceUpdatedDomainEvent priceEvent:
                    Apply(priceEvent);
                    break;
                case GameStatusUpdatedDomainEvent statusEvent:
                    Apply(statusEvent);
                    break;
                case GameRatingUpdatedDomainEvent ratingEvent:
                    Apply(ratingEvent);
                    break;
                case GameDetailsUpdatedDomainEvent detailsEvent:
                    Apply(detailsEvent);
                    break;
                case GameActivatedDomainEvent activatedEvent:
                    Apply(activatedEvent);
                    break;
                case GameDeactivatedDomainEvent deactivatedEvent:
                    Apply(deactivatedEvent);
                    break;
            }
        }

        internal void Apply(GameCreatedDomainEvent @event)
        {
            SetId(@event.AggregateId);
            Name = @event.Name;
            ReleaseDate = @event.ReleaseDate;
            AgeRating = @event.AgeRating;
            Description = @event.Description;
            DeveloperInfo = @event.DeveloperInfo;
            DiskSize = @event.DiskSize;
            Price = @event.Price;
            Playtime = @event.Playtime;
            GameDetails = @event.GameDetails;
            SystemRequirements = @event.SystemRequirements;
            Rating = @event.Rating;
            OfficialLink = @event.OfficialLink;
            GameStatus = @event.GameStatus;
            SetCreatedAt(@event.OccurredOn);
            SetActivate();
        }

        internal void Apply(GameBasicInfoUpdatedDomainEvent @event)
        {
            Name = @event.Name;
            Description = @event.Description;
            OfficialLink = @event.OfficialLink;
            SetUpdatedAt(@event.OccurredOn);
        }

        internal void Apply(GamePriceUpdatedDomainEvent @event)
        {
            Price = @event.NewPrice;
            SetUpdatedAt(@event.OccurredOn);
        }

        internal void Apply(GameStatusUpdatedDomainEvent @event)
        {
            GameStatus = @event.NewStatus;
            SetUpdatedAt(@event.OccurredOn);
        }

        internal void Apply(GameRatingUpdatedDomainEvent @event)
        {
            Rating = @event.NewRating;
            SetUpdatedAt(@event.OccurredOn);
        }

        internal void Apply(GameDetailsUpdatedDomainEvent @event)
        {
            GameDetails = @event.NewGameDetails;
            SetUpdatedAt(@event.OccurredOn);
        }

        internal void Apply(GameActivatedDomainEvent @event)
        {
            SetActivate();
            SetUpdatedAt(@event.OccurredOn);
        }

        internal void Apply(GameDeactivatedDomainEvent @event)
        {
            SetDeactivate();
            SetUpdatedAt(@event.OccurredOn);
        }

        #endregion

        #region Domain Events

        public record GameCreatedDomainEvent(
            Guid AggregateId,
            string Name,
            DateOnly ReleaseDate,
            AgeRating AgeRating,
            string? Description,
            DeveloperInfo DeveloperInfo,
            DiskSize DiskSize,
            Price Price,
            Playtime? Playtime,
            GameDetails GameDetails,
            SystemRequirements SystemRequirements,
            Rating? Rating,
            string? OfficialLink,
            string? GameStatus,
            DateTime OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameBasicInfoUpdatedDomainEvent(
            Guid AggregateId,
            string Name,
            string? Description,
            string? OfficialLink,
            DateTime OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GamePriceUpdatedDomainEvent(
            Guid AggregateId,
            Price NewPrice,
            DateTime OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameStatusUpdatedDomainEvent(
            Guid AggregateId,
            string NewStatus,
            DateTime OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameRatingUpdatedDomainEvent(
            Guid AggregateId,
            Rating? NewRating,
            DateTime OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameDetailsUpdatedDomainEvent(
            Guid AggregateId,
            GameDetails NewGameDetails,
            DateTime OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameActivatedDomainEvent(
            Guid AggregateId,
            DateTime OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record GameDeactivatedDomainEvent(
            Guid AggregateId,
            DateTime OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        #endregion
    }
}
