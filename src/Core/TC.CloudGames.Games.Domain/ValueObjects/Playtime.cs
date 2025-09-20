namespace TC.CloudGames.Games.Domain.ValueObjects
{
    /// <summary>
    /// Value Object representing playtime information with validation.
    /// </summary>
    public sealed record Playtime
    {
        public static readonly ValidationError HoursGreaterThanOrEqualToZero = new("Hours.GreaterThanOrEqualToZero", "Playtime hours must be greater than or equal to 0.");
        public static readonly ValidationError PlayerCountGreaterThanOrEqualToOne = new("PlayerCount.GreaterThanOrEqualToOne", "Player count must be greater than or equal to 1.");

        public int? Hours { get; }
        public int? PlayerCount { get; }

        private Playtime(int? hours, int? playerCount)
        {
            Hours = hours;
            PlayerCount = playerCount;
        }

        /// <summary>
        /// Validates playtime values.
        /// </summary>
        /// <param name="hours">The hours to validate (optional).</param>
        /// <param name="playerCount">The player count to validate (optional).</param>
        /// <returns>Result indicating success or validation errors.</returns>
        private static Result ValidateValue(int? hours, int? playerCount)
        {
            var errors = new List<ValidationError>();

            // Validate Hours (optional, but if provided must be >= 0)
            if (hours != null && hours < 0)
                errors.Add(HoursGreaterThanOrEqualToZero);

            // Validate PlayerCount (optional, but if provided must be >= 1)
            if (playerCount != null && playerCount < 1)
                errors.Add(PlayerCountGreaterThanOrEqualToOne);

            return errors.Count > 0 ? Result.Invalid(errors) : Result.Success();
        }

        /// <summary>
        /// Creates a new Playtime with validation.
        /// </summary>
        /// <param name="hours">The hours (optional).</param>
        /// <param name="playerCount">The player count (optional).</param>
        /// <returns>Result containing the Playtime if valid, or validation errors if invalid.</returns>
        public static Result<Playtime> Create(int? hours = null, int? playerCount = null)
        {
            var validation = ValidateValue(hours, playerCount);
            if (!validation.IsSuccess)
                return Result.Invalid(validation.ValidationErrors);

            return Result.Success(new Playtime(hours, playerCount));
        }

        /// <summary>
        /// Validates a Playtime instance.
        /// </summary>
        /// <param name="playtime">The Playtime instance to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        public static Result Validate(Playtime? playtime)
        {
            if (playtime == null)
                return Result.Success(); // Playtime itself can be null

            return ValidateValue(playtime.Hours, playtime.PlayerCount);
        }

        /// <summary>
        /// Checks if the playtime is valid.
        /// </summary>
        /// <param name="value">The Playtime instance to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValid(Playtime? value) => Validate(value).IsSuccess;

        /// <summary>
        /// Creates a Playtime value object from database data.
        /// </summary>
        /// <param name="hours">The hours from database (optional).</param>
        /// <param name="playerCount">The player count from database (optional).</param>
        /// <returns>Result containing the Playtime if valid, or validation errors if invalid.</returns>
        public static Result<Playtime> FromDb(int? hours, int? playerCount)
        {
            return Create(hours, playerCount);
        }

        /// <summary>
        /// Tries to validate a Playtime instance and returns validation errors if any.
        /// </summary>
        /// <param name="value">The Playtime instance to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidate(Playtime? value, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = Validate(value);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Validates the playtime values.
        /// </summary>
        /// <param name="hours">The hours to validate (optional).</param>
        /// <param name="playerCount">The player count to validate (optional).</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidateValue(int? hours, int? playerCount, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = ValidateValue(hours, playerCount);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Returns a string representation of the playtime.
        /// </summary>
        /// <returns>String representation of the playtime.</returns>
        public override string ToString()
        {
            var hoursText = Hours?.ToString() ?? "Unknown";
            var playerText = PlayerCount?.ToString() ?? "Unknown";
            return $"{hoursText} hours, {playerText} players";
        }
    }
}
