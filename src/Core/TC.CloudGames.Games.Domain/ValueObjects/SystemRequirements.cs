namespace TC.CloudGames.Games.Domain.ValueObjects
{
    /// <summary>
    /// Value Object representing system requirements with validation.
    /// </summary>
    public sealed record SystemRequirements
    {
        private const int MaxLength = 1000;

        public static readonly ValidationError MinimumRequired = new("Minimum.Required", "Minimum system requirements are required.");
        public static readonly ValidationError MinimumMaximumLength = new("Minimum.MaximumLength", $"Minimum system requirements must not exceed {MaxLength} characters.");
        public static readonly ValidationError RecommendedMaximumLength = new("Recommended.MaximumLength", $"Recommended system requirements must not exceed {MaxLength} characters.");

        public string Minimum { get; }
        public string? Recommended { get; }

        private SystemRequirements(string minimum, string? recommended)
        {
            Minimum = minimum;
            Recommended = recommended;
        }

        /// <summary>
        /// Validates system requirements values.
        /// </summary>
        /// <param name="minimum">The minimum system requirements to validate.</param>
        /// <param name="recommended">The recommended system requirements to validate (optional).</param>
        /// <returns>Result indicating success or validation errors.</returns>
        private static Result ValidateValue(string? minimum, string? recommended)
        {
            var errors = new List<ValidationError>();

            // Validate Minimum (required)
            if (string.IsNullOrWhiteSpace(minimum))
                errors.Add(MinimumRequired);
            else if (minimum.Length > MaxLength)
                errors.Add(MinimumMaximumLength);

            // Validate Recommended (optional, but if provided must not exceed max length)
            if (recommended != null && recommended.Length > MaxLength)
                errors.Add(RecommendedMaximumLength);

            return errors.Count > 0 ? Result.Invalid(errors) : Result.Success();
        }

        /// <summary>
        /// Creates a new SystemRequirements with validation.
        /// </summary>
        /// <param name="minimum">The minimum system requirements.</param>
        /// <param name="recommended">The recommended system requirements (optional).</param>
        /// <returns>Result containing the SystemRequirements if valid, or validation errors if invalid.</returns>
        public static Result<SystemRequirements> Create(string minimum, string? recommended = null)
        {
            var validation = ValidateValue(minimum, recommended);
            if (!validation.IsSuccess)
                return Result.Invalid(validation.ValidationErrors);

            return Result.Success(new SystemRequirements(minimum, recommended));
        }

        /// <summary>
        /// Validates a SystemRequirements instance.
        /// </summary>
        /// <param name="systemRequirements">The SystemRequirements instance to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        public static Result Validate(SystemRequirements? systemRequirements)
        {
            if (systemRequirements == null)
                return Result.Invalid(MinimumRequired);

            return ValidateValue(systemRequirements.Minimum, systemRequirements.Recommended);
        }

        /// <summary>
        /// Checks if the system requirements are valid.
        /// </summary>
        /// <param name="value">The SystemRequirements instance to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValid(SystemRequirements? value) => Validate(value).IsSuccess;

        /// <summary>
        /// Creates a SystemRequirements value object from database strings.
        /// </summary>
        /// <param name="minimum">The minimum system requirements from database.</param>
        /// <param name="recommended">The recommended system requirements from database (optional).</param>
        /// <returns>Result containing the SystemRequirements if valid, or validation errors if invalid.</returns>
        public static Result<SystemRequirements> FromDb(string minimum, string? recommended = null)
        {
            if (string.IsNullOrWhiteSpace(minimum))
                return Result.Invalid(MinimumRequired);

            return Result.Success(new SystemRequirements(minimum, recommended));
        }

        /// <summary>
        /// Tries to validate a SystemRequirements instance and returns validation errors if any.
        /// </summary>
        /// <param name="value">The SystemRequirements instance to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidate(SystemRequirements? value, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = Validate(value);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Validates the system requirements text values.
        /// </summary>
        /// <param name="minimum">The minimum system requirements to validate.</param>
        /// <param name="recommended">The recommended system requirements to validate (optional).</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidateValue(string? minimum, string? recommended, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = ValidateValue(minimum, recommended);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Returns a string representation of the system requirements.
        /// </summary>
        /// <returns>String representation of the system requirements.</returns>
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Recommended) ? Minimum : $"{Minimum} (Recommended: {Recommended})";
        }
    }
}
