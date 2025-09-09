using Ardalis.Result;

namespace TC.CloudGames.Games.Domain.ValueObjects
{
    /// <summary>
    /// Value Object representing disk size in GB with validation.
    /// </summary>
    public sealed record DiskSize
    {
        public static readonly ValidationError Required = new("DiskSize.Required", "Disk size value is required.");
        public static readonly ValidationError GreaterThanZero = new("DiskSize.GreaterThanZero", "Disk size value must be greater than 0.");

        public decimal SizeInGb { get; }

        private DiskSize(decimal sizeInGb)
        {
            SizeInGb = sizeInGb;
        }

        /// <summary>
        /// Validates a disk size value.
        /// </summary>
        /// <param name="sizeInGb">The disk size value in GB to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        private static Result ValidateValue(decimal? sizeInGb)
        {
            if (sizeInGb == null || sizeInGb == 0)
                return Result.Invalid(Required);

            if (sizeInGb <= 0)
                return Result.Invalid(GreaterThanZero);

            return Result.Success();
        }

        /// <summary>
        /// Creates a new DiskSize with validation.
        /// </summary>
        /// <param name="sizeInGb">The disk size value in GB to validate.</param>
        /// <returns>Result containing the DiskSize if valid, or validation errors if invalid.</returns>
        public static Result<DiskSize> Create(decimal sizeInGb)
        {
            var validation = ValidateValue(sizeInGb);
            if (!validation.IsSuccess)
                return Result.Invalid(validation.ValidationErrors);

            return Result.Success(new DiskSize(sizeInGb));
        }

        /// <summary>
        /// Validates a DiskSize instance.
        /// </summary>
        /// <param name="diskSize">The DiskSize instance to validate.</param>
        /// <returns>Result indicating success or validation errors.</returns>
        public static Result Validate(DiskSize? diskSize)
        {
            if (diskSize == null)
                return Result.Invalid(Required);

            return ValidateValue(diskSize.SizeInGb);
        }

        /// <summary>
        /// Checks if the disk size value is valid.
        /// </summary>
        /// <param name="value">The DiskSize instance to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValid(DiskSize? value) => Validate(value).IsSuccess;

        /// <summary>
        /// Creates a DiskSize value object from a database decimal.
        /// </summary>
        /// <param name="sizeInGb">The disk size value from database.</param>
        /// <returns>Result containing the DiskSize if valid, or validation errors if invalid.</returns>
        public static Result<DiskSize> FromDb(decimal sizeInGb)
        {
            if (sizeInGb <= 0)
                return Result.Invalid(GreaterThanZero);

            return Result.Success(new DiskSize(sizeInGb));
        }

        /// <summary>
        /// Tries to validate a DiskSize instance and returns validation errors if any.
        /// </summary>
        /// <param name="value">The DiskSize instance to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidate(DiskSize? value, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = Validate(value);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Validates the disk size decimal value.
        /// </summary>
        /// <param name="sizeInGb">The disk size value to validate.</param>
        /// <param name="errors">List of validation errors if any.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public static bool TryValidateValue(decimal? sizeInGb, out IReadOnlyCollection<ValidationError> errors)
        {
            var result = ValidateValue(sizeInGb);
            errors = !result.IsSuccess ? [.. result.ValidationErrors] : [];
            return result.IsSuccess;
        }

        /// <summary>
        /// Implicit conversion from DiskSize to decimal.
        /// </summary>
        /// <param name="diskSize">The DiskSize instance.</param>
        public static implicit operator decimal(DiskSize diskSize) => diskSize.SizeInGb;
    }
}
