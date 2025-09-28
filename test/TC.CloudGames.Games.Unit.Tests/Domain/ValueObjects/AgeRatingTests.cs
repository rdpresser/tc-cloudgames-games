using TC.CloudGames.Games.Domain.ValueObjects;

namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class AgeRatingTests
    {
        [Theory]
        [InlineData("E", true)]
        [InlineData("E10+", true)]
        [InlineData("T", true)]
        [InlineData("M", true)]
        [InlineData("AO", true)]
        [InlineData("", false)]
        [InlineData("Invalid", false)]
        public void Create_WithValidAgeRating_ShouldReturnSuccess(string ageRating, bool expectedSuccess)
        {
            // Act
            var result = AgeRating.Create(ageRating);

            // Assert
            if (expectedSuccess)
            {
                result.IsSuccess.ShouldBeTrue();
                result.Value.Value.ShouldBe(ageRating);
            }
            else
            {
                result.IsSuccess.ShouldBeFalse();
                result.ValidationErrors.ShouldNotBeEmpty();
            }
        }

        [Fact]
        public void FromDb_WithValidAgeRating_ShouldCreateValueObject()
        {
            // Arrange
            var ageRating = "E";

            // Act
            var result = AgeRating.FromDb(ageRating);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Value.ShouldBe(ageRating);
        }

        [Fact]
        public void TryValidate_WithValidAgeRating_ShouldReturnTrue()
        {
            // Arrange
            var ageRating = AgeRating.Create("E").Value;

            // Act
            var isValid = AgeRating.TryValidate(ageRating, out var errors);

            // Assert
            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidate_WithInvalidAgeRating_ShouldReturnFalse()
        {
            // Arrange
            var ageRating = new AgeRating(""); // Invalid age rating

            // Act
            var isValid = AgeRating.TryValidate(ageRating, out var errors);

            // Assert
            isValid.ShouldBeFalse();
            errors.ShouldNotBeEmpty();
        }
    }
}

