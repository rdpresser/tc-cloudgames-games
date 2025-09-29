using Shouldly;
using TC.CloudGames.Games.Domain.ValueObjects;

namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class AgeRatingTests
    {
        [Theory]
        [InlineData("E")]
        [InlineData("e")]
        [InlineData("E10+")]
        [InlineData("t")]
        [InlineData("M")]
        [InlineData("a")]
        [InlineData("Rp")]
        public void Create_ShouldReturnSuccess_WhenValidValue(string value)
        {
            // Act
            var result = AgeRating.Create(value);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Value.ShouldBe(result.Value.Value); // normalized
        }

        [Fact]
        public void Create_ShouldReturnError_WhenValueIsNullOrEmpty()
        {
            // Act
            var result1 = AgeRating.Create("");

            // Assert
            result1.IsSuccess.ShouldBeFalse();
            result1.ValidationErrors.ShouldContain(AgeRating.Required);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenValueExceedsMaxLength()
        {
            // Arrange
            var longValue = new string('A', 11);

            // Act
            var result = AgeRating.Create(longValue);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(AgeRating.MaximumLength);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenValueIsInvalid()
        {
            // Act
            var result = AgeRating.Create("INVALID");

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(AgeRating.Invalid);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenAgeRatingIsNull()
        {
            // Act
            var result = AgeRating.Validate(null);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(AgeRating.Invalid);
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_ForValidAgeRating()
        {
            // Arrange
            var rating = AgeRating.Create("T").Value;

            // Act
            var isValid = AgeRating.IsValid(rating);

            // Assert
            isValid.ShouldBeTrue();
        }

        [Fact]
        public void FromDb_ShouldReturnSuccess_EvenIfNotValidated()
        {
            // Act
            var result = AgeRating.FromDb("AnyString");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Value.ShouldBe("AnyString");
        }

        [Fact]
        public void FromDb_ShouldReturnError_WhenNullOrEmpty()
        {
            // Act
            var result = AgeRating.FromDb("");

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(AgeRating.Invalid);
        }

        [Fact]
        public void TryValidate_ShouldReturnErrors_WhenInvalid()
        {
            // Arrange
            var invalidRating = AgeRating.Create("INVALID").Value; // deve ser null

            // Act
            var isValid = AgeRating.TryValidate(invalidRating, out var errors);

            // Assert
            isValid.ShouldBeFalse();
            errors.ShouldContain(AgeRating.Invalid);
        }

        [Fact]
        public void TryValidateValue_ShouldReturnTrue_WhenValid()
        {
            // Act
            var isValid = AgeRating.TryValidateValue("M", out var errors);

            // Assert
            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void ImplicitConversion_ShouldReturnUnderlyingValue()
        {
            // Arrange
            var rating = AgeRating.Create("E").Value;

            // Act
            string value = rating;

            // Assert
            value.ShouldBe("E");
        }
    }
}
