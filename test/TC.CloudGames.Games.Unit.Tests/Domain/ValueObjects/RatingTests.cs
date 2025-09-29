using Shouldly;
using TC.CloudGames.Games.Domain.ValueObjects;

namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class RatingTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenValueIsNull()
        {
            // Act
            var result = Rating.Create();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Average.ShouldBeNull();
        }

        [Fact]
        public void Create_ShouldReturnSuccess_WhenValueIsBetweenZeroAndTen()
        {
            // Act
            var result1 = Rating.Create(0);
            var result2 = Rating.Create(7.5m);
            var result3 = Rating.Create(10);

            // Assert
            result1.IsSuccess.ShouldBeTrue();
            result1.Value.Average.ShouldBe(0);

            result2.IsSuccess.ShouldBeTrue();
            result2.Value.Average.ShouldBe(7.5m);

            result3.IsSuccess.ShouldBeTrue();
            result3.Value.Average.ShouldBe(10);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenValueIsNegative()
        {
            // Act
            var result = Rating.Create(-1);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(Rating.GreaterThanOrEqualToZero);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenValueIsGreaterThanTen()
        {
            // Act
            var result = Rating.Create(11);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(Rating.LessThanOrEqualToTen);
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenInstanceIsNull()
        {
            // Act
            var result = Rating.Validate(null);

            // Assert
            result.IsSuccess.ShouldBeTrue(); // null é permitido
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenInstanceIsValid()
        {
            var rating = Rating.Create(8).Value;

            var result = Rating.Validate(rating);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenRatingIsValid()
        {
            var rating = Rating.Create(5).Value;

            var isValid = Rating.IsValid(rating);

            isValid.ShouldBeTrue();
        }

        [Fact]
        public void FromDb_ShouldReturnSuccess_WhenValueIsValid()
        {
            var result = Rating.FromDb(9.5m);

            result.IsSuccess.ShouldBeTrue();
            result.Value.Average.ShouldBe(9.5m);
        }

        [Fact]
        public void FromDb_ShouldReturnError_WhenValueIsInvalid()
        {
            var result = Rating.FromDb(15);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(Rating.LessThanOrEqualToTen);
        }

        [Fact]
        public void TryValidate_ShouldReturnTrue_WhenValid()
        {
            var rating = Rating.Create(6).Value;

            var isValid = Rating.TryValidate(rating, out var errors);

            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidateValue_ShouldReturnTrue_WhenValid()
        {
            var isValid = Rating.TryValidateValue(7, out var errors);

            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidateValue_ShouldReturnErrors_WhenInvalid()
        {
            var isValid = Rating.TryValidateValue(20, out var errors);

            isValid.ShouldBeFalse();
            errors.ShouldContain(Rating.LessThanOrEqualToTen);
        }

        [Fact]
        public void ImplicitConversion_ShouldReturnDecimalValue()
        {
            var rating = Rating.Create(4.5m).Value;

            decimal? value = rating;

            value.ShouldBe(4.5m);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedValue_WhenHasRating()
        {
            var rating = Rating.Create(9).Value;

            var str = rating.ToString();

            str.ShouldBe("Rating: 9");
        }

        [Fact]
        public void ToString_ShouldReturnNoRating_WhenValueIsNull()
        {
            var rating = Rating.Create().Value;

            var str = rating.ToString();

            str.ShouldBe("No Rating");
        }
    }
}
