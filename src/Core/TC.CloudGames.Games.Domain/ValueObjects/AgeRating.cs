using Ardalis.Result;
using System.Collections.Immutable;
using TC.CloudGames.SharedKernel.Extensions;

namespace TC.CloudGames.Games.Domain.ValueObjects
{
    /// <summary>
    /// Value Object representing an age rating with validation and predefined ratings.
    /// </summary>
    public sealed class AgeRating
    {
        private const int MaxLength = 10;

        public static readonly IImmutableSet<string> ValidRatings = ImmutableHashSet.Create("E", "E10+", "T", "M", "A", "RP");
        
        public static readonly ValidationError Required = new("AgeRating.Required", "Age rating value is required.");
        public static readonly ValidationError Invalid = new("AgeRating.Invalid", $"Invalid age rating value specified. Valid age ratings are: {ValidRatings.JoinWithQuotes()}.");
        public static readonly ValidationError MaximumLength = new("AgeRating.MaximumLength", $"Age rating value cannot exceed {MaxLength} characters.");

        public string Value { get; }

        private AgeRating(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Validates an age rating value.
        /// </summary>
        /// <param name="value">The age rating value to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        private static Result ValidateValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Invalid(Required);

            if (value.Length > MaxLength)
                return Result.Invalid(MaximumLength);

            var normalizedValue = ValidRatings.FirstOrDefault(r =>
                string.Equals(r, value, StringComparison.OrdinalIgnoreCase));

            if (normalizedValue == null)
                return Result.Invalid(Invalid);

            return Result.Success();
        }

        /// <summary>
        /// Creates a new AgeRating with validation.
        /// </summary>
        /// <param name="value">The age rating value to validate.</param>
        /// <returns>Result containing the AgeRating if valid, or validation errors if invalid.</returns>
        public static Result<AgeRating> Create(string value)
        {
            var validation = ValidateValue(value);
            if (!validation.IsSuccess)
                return Result.Invalid(validation.ValidationErrors);

            var normalizedValue = ValidRatings.First(r =>
                string.Equals(r, value, StringComparison.OrdinalIgnoreCase));

            return Result.Success(new AgeRating(normalizedValue));
        }

        /// <summary>
        /// Validates an AgeRating instance.
        /// </summary>
        /// <param name="ageRating">The AgeRating instance to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        public static Result Validate(AgeRating? ageRating)
        {
            if (ageRating == null)
                return Result.Invalid(Invalid);

            return ValidateValue(ageRating.Value);
        }

        /// <summary>
        /// Checks if the age rating value is valid.
        /// </summary>
        /// <param name="value">The AgeRating instance to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValid(AgeRating? value) => Validate(value).IsSuccess;

        /// <summary>
        /// Creates an AgeRating value object from a database string.
        /// </summary>
        /// <param name="value">The age rating value from database.</param>
        /// <returns>Result containing the AgeRating if valid, or validation errors if invalid.</returns>
        public static Result<AgeRating> FromDb(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Invalid(Invalid);

            return Result.Success(new AgeRating(value));
        }

        /// <summary>
        /// Tries to validate an AgeRating instance and returns validation errors if any.
        /// </summary>
        /// <param name="value">The AgeRating instance to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidate(AgeRating? value, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = Validate(value);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Validates the age rating text value.
        /// </summary>
        /// <param name="value">The text age rating to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidateValue(string? value, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = ValidateValue(value);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Implicit conversion from AgeRating to string.
        /// </summary>
        /// <param name="ageRating">The AgeRating instance.</param>
        public static implicit operator string(AgeRating ageRating) => ageRating.Value;
    }
}
