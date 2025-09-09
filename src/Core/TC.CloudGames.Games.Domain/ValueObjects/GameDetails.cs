using Ardalis.Result;
using System.Collections.Immutable;
using System.Text.Json;
using TC.CloudGames.SharedKernel.Extensions;

namespace TC.CloudGames.Games.Domain.ValueObjects
{
    /// <summary>
    /// Value Object representing game details with validation.
    /// </summary>
    public sealed record GameDetails
    {
        private const int GenreMaxLength = 50;
        private const int TagsMaxLength = 200;
        private const int AvailableLanguagesMaxLength = 100;

        public static readonly IImmutableSet<string> ValidPlatforms = ImmutableHashSet.Create(
            "PC", "PlayStation 4", "PlayStation 5", "Xbox One", "Xbox Series X|S", "Nintendo Switch",
            "Nintendo 3DS", "Wii U", "PlayStation Vita", "Android", "iOS", "macOS", "Linux", "Stadia", "Steam Deck", "Browser",
            "VR (Oculus Quest)", "VR (HTC Vive)", "VR (PlayStation VR)"
        );

        public static readonly IImmutableSet<string> ValidGameModes = ImmutableHashSet.Create(
            "Singleplayer", "Multiplayer", "Co-op", "PvP", "PvE", "Battle Royale", "Survival",
            "Sandbox", "Casual"
        );

        public static readonly IImmutableSet<string> ValidDistributionFormats = ImmutableHashSet.Create(
            "Digital", "Physical"
        );

        public static readonly ValidationError PlatformRequired = new("Platform.Required", "Platform is required.");
        public static readonly ValidationError PlatformInvalid = new("Platform.ValidPlatform", $"Invalid platform specified. Valid platforms are: {ValidPlatforms.JoinWithQuotes()}.");
        public static readonly ValidationError GameModeRequired = new("GameMode.Required", "Game mode is required.");
        public static readonly ValidationError GameModeInvalid = new("GameMode.ValidGameMode", $"Invalid game mode specified. Valid game modes are: {ValidGameModes.JoinWithQuotes()}.");
        public static readonly ValidationError DistributionFormatRequired = new("DistributionFormat.Required", "Distribution format is required.");
        public static readonly ValidationError DistributionFormatInvalid = new("DistributionFormat.ValidDistributionFormat", $"Invalid distribution format specified. Valid formats are: {ValidDistributionFormats.JoinWithQuotes()}.");
        public static readonly ValidationError GenreMaximumLength = new("Genre.MaximumLength", $"Genre must not exceed {GenreMaxLength} characters.");
        public static readonly ValidationError TagsMaximumLength = new("Tags.MaximumLength", $"Tags must not exceed {TagsMaxLength} characters.");
        public static readonly ValidationError AvailableLanguagesMaximumLength = new("AvailableLanguages.MaximumLength", $"Available languages must not exceed {AvailableLanguagesMaxLength} characters.");
        public static readonly ValidationError SupportsDlcsRequired = new("SupportsDlcs.Required", "Supports DLCs field is required.");

        public string? Genre { get; }
        public IReadOnlyCollection<string> PlatformList { get; }
        public string? Tags { get; }
        public string GameMode { get; }
        public string DistributionFormat { get; }
        public string? AvailableLanguages { get; }
        public bool SupportsDlcs { get; }

        private GameDetails(
            string? genre,
            IReadOnlyCollection<string> platformList,
            string? tags,
            string gameMode,
            string distributionFormat,
            string? availableLanguages,
            bool supportsDlcs)
        {
            Genre = genre;
            PlatformList = platformList;
            Tags = tags;
            GameMode = gameMode;
            DistributionFormat = distributionFormat;
            AvailableLanguages = availableLanguages;
            SupportsDlcs = supportsDlcs;
        }

        /// <summary>
        /// Validates game details values.
        /// </summary>
        /// <param name="genre">The genre to validate (optional).</param>
        /// <param name="platformList">The platform list to validate.</param>
        /// <param name="tags">The tags to validate (optional).</param>
        /// <param name="gameMode">The game mode to validate.</param>
        /// <param name="distributionFormat">The distribution format to validate.</param>
        /// <param name="availableLanguages">The available languages to validate (optional).</param>
        /// <param name="supportsDlcs">The supports DLCs flag to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        private static Result ValidateValue(
            string? genre,
            IReadOnlyCollection<string>? platformList,
            string? tags,
            string? gameMode,
            string? distributionFormat,
            string? availableLanguages,
            bool? supportsDlcs)
        {
            var errors = new List<ValidationError>();

            // Validate Genre (optional, but if provided must not exceed max length)
            if (genre != null && genre.Length > GenreMaxLength)
                errors.Add(GenreMaximumLength);

            // Validate Platform (required and must be valid)
            if (platformList == null || !platformList.Any())
                errors.Add(PlatformRequired);
            else if (platformList.Any(platform => !ValidPlatforms.Contains(platform)))
                errors.Add(PlatformInvalid);

            // Validate Tags (optional, but if provided must not exceed max length)
            if (tags != null && tags.Length > TagsMaxLength)
                errors.Add(TagsMaximumLength);

            // Validate GameMode (required and must be valid)
            if (string.IsNullOrWhiteSpace(gameMode))
                errors.Add(GameModeRequired);
            else if (!ValidGameModes.Contains(gameMode))
                errors.Add(GameModeInvalid);

            // Validate DistributionFormat (required and must be valid)
            if (string.IsNullOrWhiteSpace(distributionFormat))
                errors.Add(DistributionFormatRequired);
            else if (!ValidDistributionFormats.Contains(distributionFormat))
                errors.Add(DistributionFormatInvalid);

            // Validate AvailableLanguages (optional, but if provided must not exceed max length)
            if (availableLanguages != null && availableLanguages.Length > AvailableLanguagesMaxLength)
                errors.Add(AvailableLanguagesMaximumLength);

            // Validate SupportsDlcs (required)
            if (supportsDlcs == null)
                errors.Add(SupportsDlcsRequired);

            return errors.Count > 0 ? Result.Invalid(errors) : Result.Success();
        }

        /// <summary>
        /// Creates a new GameDetails with validation.
        /// </summary>
        /// <param name="genre">The genre (optional).</param>
        /// <param name="platformList">The platform list.</param>
        /// <param name="tags">The tags (optional).</param>
        /// <param name="gameMode">The game mode.</param>
        /// <param name="distributionFormat">The distribution format.</param>
        /// <param name="availableLanguages">The available languages (optional).</param>
        /// <param name="supportsDlcs">The supports DLCs flag.</param>
        /// <returns>Result containing the GameDetails if valid, or validation errors if invalid.</returns>
        public static Result<GameDetails> Create(
            string? genre,
            IReadOnlyCollection<string> platformList,
            string? tags,
            string gameMode,
            string distributionFormat,
            string? availableLanguages,
            bool supportsDlcs)
        {
            var validation = ValidateValue(genre, platformList, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs);
            if (!validation.IsSuccess)
                return Result.Invalid(validation.ValidationErrors);

            return Result.Success(new GameDetails(genre, platformList, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs));
        }

        /// <summary>
        /// Validates a GameDetails instance.
        /// </summary>
        /// <param name="gameDetails">The GameDetails instance to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        public static Result Validate(GameDetails? gameDetails)
        {
            if (gameDetails == null)
                return Result.Invalid(PlatformRequired, GameModeRequired, DistributionFormatRequired, SupportsDlcsRequired);

            return ValidateValue(
                gameDetails.Genre,
                gameDetails.PlatformList,
                gameDetails.Tags,
                gameDetails.GameMode,
                gameDetails.DistributionFormat,
                gameDetails.AvailableLanguages,
                gameDetails.SupportsDlcs);
        }

        /// <summary>
        /// Checks if the game details are valid.
        /// </summary>
        /// <param name="value">The GameDetails instance to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValid(GameDetails? value) => Validate(value).IsSuccess;

        /// <summary>
        /// Creates a GameDetails value object from database data.
        /// </summary>
        /// <param name="genre">The genre from database.</param>
        /// <param name="platformListJson">The platform list JSON from database.</param>
        /// <param name="tags">The tags from database.</param>
        /// <param name="gameMode">The game mode from database.</param>
        /// <param name="distributionFormat">The distribution format from database.</param>
        /// <param name="availableLanguages">The available languages from database.</param>
        /// <param name="supportsDlcs">The supports DLCs flag from database.</param>
        /// <returns>Result containing the GameDetails if valid, or validation errors if invalid.</returns>
        public static Result<GameDetails> FromDb(
            string? genre,
            string platformListJson,
            string? tags,
            string gameMode,
            string distributionFormat,
            string? availableLanguages,
            bool supportsDlcs)
        {
            var platformList = string.IsNullOrWhiteSpace(platformListJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(platformListJson) ?? new List<string>();

            return Create(genre, platformList, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs);
        }

        /// <summary>
        /// Tries to validate a GameDetails instance and returns validation errors if any.
        /// </summary>
        /// <param name="value">The GameDetails instance to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidate(GameDetails? value, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = Validate(value);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Validates the game details values.
        /// </summary>
        /// <param name="genre">The genre to validate (optional).</param>
        /// <param name="platformList">The platform list to validate.</param>
        /// <param name="tags">The tags to validate (optional).</param>
        /// <param name="gameMode">The game mode to validate.</param>
        /// <param name="distributionFormat">The distribution format to validate.</param>
        /// <param name="availableLanguages">The available languages to validate (optional).</param>
        /// <param name="supportsDlcs">The supports DLCs flag to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidateValue(
            string? genre,
            IReadOnlyCollection<string>? platformList,
            string? tags,
            string? gameMode,
            string? distributionFormat,
            string? availableLanguages,
            bool? supportsDlcs,
            out IReadOnlyCollection<ValidationError> errors)
        {
            var result = ValidateValue(genre, platformList, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Gets the platform list as JSON string for database storage.
        /// </summary>
        /// <returns>JSON string representation of the platform list.</returns>
        public string GetPlatformListAsJson() => JsonSerializer.Serialize(PlatformList);

        /// <summary>
        /// Returns a string representation of the game details.
        /// </summary>
        /// <returns>String representation of the game details.</returns>
        public override string ToString()
        {
            var platforms = string.Join(", ", PlatformList);
            return $"{platforms} - {GameMode} - {DistributionFormat}";
        }
    }
}
