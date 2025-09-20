namespace TC.CloudGames.Games.Domain.ValueObjects
{
    /// <summary>
    /// Value Object representing developer and publisher information with validation.
    /// </summary>
    public sealed record DeveloperInfo
    {
        private const int DeveloperMaxLength = 100;
        private const int PublisherMaxLength = 200;

        public static readonly ValidationError DeveloperRequired = new("Developer.Required", "Developer name is required.");
        public static readonly ValidationError DeveloperMaximumLength = new("Developer.MaximumLength", $"Developer name must not exceed {DeveloperMaxLength} characters.");
        public static readonly ValidationError PublisherMaximumLength = new("Publisher.MaximumLength", $"Publisher name must not exceed {PublisherMaxLength} characters.");

        public string Developer { get; }
        public string? Publisher { get; }

        private DeveloperInfo(string developer, string? publisher)
        {
            Developer = developer;
            Publisher = publisher;
        }

        /// <summary>
        /// Validates developer and publisher information.
        /// </summary>
        /// <param name="developer">The developer name to validate.</param>
        /// <param name="publisher">The publisher name to validate (optional).</param>
        /// <returns>Result indicating success or validation errors.</returns>
        private static Result ValidateValue(string? developer, string? publisher)
        {
            var errors = new List<ValidationError>();

            // Validate developer
            if (string.IsNullOrWhiteSpace(developer))
                errors.Add(DeveloperRequired);
            else if (developer.Length > DeveloperMaxLength)
                errors.Add(DeveloperMaximumLength);

            // Validate publisher (only if not null)
            if (publisher != null && publisher.Length > PublisherMaxLength)
                errors.Add(PublisherMaximumLength);

            return errors.Count > 0 ? Result.Invalid(errors) : Result.Success();
        }

        /// <summary>
        /// Creates a new DeveloperInfo with validation.
        /// </summary>
        /// <param name="developer">The developer name.</param>
        /// <param name="publisher">The publisher name (optional).</param>
        /// <returns>Result containing the DeveloperInfo if valid, or validation errors if invalid.</returns>
        public static Result<DeveloperInfo> Create(string developer, string? publisher = null)
        {
            var validation = ValidateValue(developer, publisher);
            if (!validation.IsSuccess)
                return Result.Invalid(validation.ValidationErrors);

            return Result.Success(new DeveloperInfo(developer, publisher));
        }

        /// <summary>
        /// Validates a DeveloperInfo instance.
        /// </summary>
        /// <param name="developerInfo">The DeveloperInfo instance to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        public static Result Validate(DeveloperInfo? developerInfo)
        {
            if (developerInfo == null)
                return Result.Invalid(DeveloperRequired);

            return ValidateValue(developerInfo.Developer, developerInfo.Publisher);
        }

        /// <summary>
        /// Checks if the developer info is valid.
        /// </summary>
        /// <param name="value">The DeveloperInfo instance to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValid(DeveloperInfo? value) => Validate(value).IsSuccess;

        /// <summary>
        /// Creates a DeveloperInfo value object from database strings.
        /// </summary>
        /// <param name="developer">The developer name from database.</param>
        /// <param name="publisher">The publisher name from database (optional).</param>
        /// <returns>Result containing the DeveloperInfo if valid, or validation errors if invalid.</returns>
        public static Result<DeveloperInfo> FromDb(string developer, string? publisher = null)
        {
            if (string.IsNullOrWhiteSpace(developer))
                return Result.Invalid(DeveloperRequired);

            return Result.Success(new DeveloperInfo(developer, publisher));
        }

        /// <summary>
        /// Tries to validate a DeveloperInfo instance and returns validation errors if any.
        /// </summary>
        /// <param name="value">The DeveloperInfo instance to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidate(DeveloperInfo? value, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = Validate(value);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Validates the developer and publisher text values.
        /// </summary>
        /// <param name="developer">The developer name to validate.</param>
        /// <param name="publisher">The publisher name to validate (optional).</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidateValue(string? developer, string? publisher, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = ValidateValue(developer, publisher);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }
    }
}
