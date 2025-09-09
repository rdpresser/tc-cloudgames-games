using Ardalis.Result;

namespace TC.CloudGames.Games.Domain.ValueObjects
{
    /// <summary>
    /// Value Object representing a price with validation.
    /// </summary>
    public sealed record Price
    {
        public static readonly ValidationError Required = new("Price.Required", "Price amount is required.");
        public static readonly ValidationError GreaterThanOrEqualToZero = new("Price.GreaterThanOrEqualToZero", "Price amount must be greater than or equal to 0.");

        public decimal Amount { get; }

        private Price(decimal amount)
        {
            Amount = amount;
        }

        /// <summary>
        /// Validates a price amount value.
        /// </summary>
        /// <param name="amount">The price amount to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        private static Result ValidateValue(decimal? amount)
        {
            if (amount == null)
                return Result.Invalid(Required);

            if (amount < 0)
                return Result.Invalid(GreaterThanOrEqualToZero);

            return Result.Success();
        }

        /// <summary>
        /// Creates a new Price with validation.
        /// </summary>
        /// <param name="amount">The price amount to validate.</param>
        /// <returns>Result containing the Price if valid, or validation errors if invalid.</returns>
        public static Result<Price> Create(decimal amount)
        {
            var validation = ValidateValue(amount);
            if (!validation.IsSuccess)
                return Result.Invalid(validation.ValidationErrors);

            return Result.Success(new Price(amount));
        }

        /// <summary>
        /// Validates a Price instance.
        /// </summary>
        /// <param name="price">The Price instance to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        public static Result Validate(Price? price)
        {
            if (price == null)
                return Result.Invalid(Required);

            return ValidateValue(price.Amount);
        }

        /// <summary>
        /// Checks if the price is valid.
        /// </summary>
        /// <param name="value">The Price instance to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValid(Price? value) => Validate(value).IsSuccess;

        /// <summary>
        /// Creates a Price value object from a database decimal.
        /// </summary>
        /// <param name="amount">The price amount from database.</param>
        /// <returns>Result containing the Price if valid, or validation errors if invalid.</returns>
        public static Result<Price> FromDb(decimal amount)
        {
            if (amount < 0)
                return Result.Invalid(GreaterThanOrEqualToZero);

            return Result.Success(new Price(amount));
        }

        /// <summary>
        /// Tries to validate a Price instance and returns validation errors if any.
        /// </summary>
        /// <param name="value">The Price instance to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidate(Price? value, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = Validate(value);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Validates the price amount value.
        /// </summary>
        /// <param name="amount">The price amount to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidateValue(decimal? amount, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = ValidateValue(amount);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Implicit conversion from Price to decimal.
        /// </summary>
        /// <param name="price">The Price instance.</param>
        public static implicit operator decimal(Price price) => price.Amount;

        /// <summary>
        /// Returns a formatted string representation of the price.
        /// </summary>
        /// <returns>Currency formatted string representation of the price.</returns>
        public override string ToString()
        {
            return $"{Amount:C}";
        }
    }
}
