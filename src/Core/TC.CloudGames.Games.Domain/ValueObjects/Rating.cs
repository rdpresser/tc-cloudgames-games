using Ardalis.Result;

namespace TC.CloudGames.Games.Domain.ValueObjects
{
    /// <summary>
    /// Value Object representing a rating with validation.
    /// </summary>
    public sealed record Rating
    {
        public static readonly ValidationError GreaterThanOrEqualToZero = new("Rating.GreaterThanOrEqualToZero", "Rating value must be greater than or equal to 0.");
        public static readonly ValidationError LessThanOrEqualToTen = new("Rating.LessThanOrEqualToTen", "Rating value must be less than or equal to 10.");

        public decimal? Average { get; }

        private Rating(decimal? average)
        {
            Average = average;
        }

        /// <summary>
        /// Validates a rating average value.
        /// </summary>
        /// <param name="average">The rating average to validate (optional).</param>
        /// <returns>Result indicating success or validation errors.</returns>
        private static Result ValidateValue(decimal? average)
        {
            var errors = new List<ValidationError>();

            // Validate Average (optional, but if provided must be between 0 and 10)
            if (average != null)
            {
                if (average < 0)
                    errors.Add(GreaterThanOrEqualToZero);

                if (average > 10)
                    errors.Add(LessThanOrEqualToTen);
            }

            return errors.Count > 0 ? Result.Invalid(errors) : Result.Success();
        }

        /// <summary>
        /// Creates a new Rating with validation.
        /// </summary>
        /// <param name="average">The rating average (optional).</param>
        /// <returns>Result containing the Rating if valid, or validation errors if invalid.</returns>
        public static Result<Rating> Create(decimal? average = null)
        {
            var validation = ValidateValue(average);
            if (!validation.IsSuccess)
                return Result.Invalid(validation.ValidationErrors);

            return Result.Success(new Rating(average));
        }

        /// <summary>
        /// Validates a Rating instance.
        /// </summary>
        /// <param name="rating">The Rating instance to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        public static Result Validate(Rating? rating)
        {
            if (rating == null)
                return Result.Success(); // Rating itself can be null

            return ValidateValue(rating.Average);
        }

        /// <summary>
        /// Checks if the rating is valid.
        /// </summary>
        /// <param name="value">The Rating instance to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValid(Rating? value) => Validate(value).IsSuccess;

        /// <summary>
        /// Creates a Rating value object from a database decimal.
        /// </summary>
        /// <param name="average">The rating average from database (optional).</param>
        /// <returns>Result containing the Rating if valid, or validation errors if invalid.</returns>
        public static Result<Rating> FromDb(decimal? average)
        {
            return Create(average);
        }

        /// <summary>
        /// Tries to validate a Rating instance and returns validation errors if any.
        /// </summary>
        /// <param name="value">The Rating instance to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidate(Rating? value, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = Validate(value);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Validates the rating average value.
        /// </summary>
        /// <param name="average">The rating average to validate (optional).</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidateValue(decimal? average, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = ValidateValue(average);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Implicit conversion from Rating to decimal.
        /// </summary>
        /// <param name="rating">The Rating instance.</param>
        public static implicit operator decimal?(Rating? rating) => rating?.Average;

        /// <summary>
        /// Returns a string representation of the rating.
        /// </summary>
        /// <returns>String representation of the rating.</returns>
        public override string ToString()
        {
            return Average.HasValue ? $"Rating: {Average.Value}" : "No Rating";
        }
    }
}
