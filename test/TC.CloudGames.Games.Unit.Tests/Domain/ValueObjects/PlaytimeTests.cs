using Shouldly;
using TC.CloudGames.Games.Domain.ValueObjects;

namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class PlaytimeTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenValuesAreNull()
        {
            // Act
            var result = Playtime.Create();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Hours.ShouldBeNull();
            result.Value.PlayerCount.ShouldBeNull();
        }

        [Fact]
        public void Create_ShouldReturnSuccess_WhenValuesAreValid()
        {
            // Act
            var result = Playtime.Create(10, 2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Hours.ShouldBe(10);
            result.Value.PlayerCount.ShouldBe(2);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenHoursIsNegative()
        {
            // Act
            var result = Playtime.Create(-5, 2);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(Playtime.HoursGreaterThanOrEqualToZero);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenPlayerCountIsLessThanOne()
        {
            // Act
            var result = Playtime.Create(5, 0);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(Playtime.PlayerCountGreaterThanOrEqualToOne);
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenInstanceIsNull()
        {
            // Act
            var result = Playtime.Validate(null);

            // Assert
            result.IsSuccess.ShouldBeTrue(); // null é permitido
        }


        [Fact]
        public void IsValid_ShouldReturnTrue_WhenValid()
        {
            var playtime = Playtime.Create(20, 1).Value;

            var isValid = Playtime.IsValid(playtime);

            isValid.ShouldBeTrue();
        }

        [Fact]
        public void FromDb_ShouldReturnSuccess_WhenValuesAreValid()
        {
            var result = Playtime.FromDb(15, 4);

            result.IsSuccess.ShouldBeTrue();
            result.Value.Hours.ShouldBe(15);
            result.Value.PlayerCount.ShouldBe(4);
        }

        [Fact]
        public void FromDb_ShouldReturnError_WhenValuesAreInvalid()
        {
            var result = Playtime.FromDb(-3, 0);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(Playtime.HoursGreaterThanOrEqualToZero);
            result.ValidationErrors.ShouldContain(Playtime.PlayerCountGreaterThanOrEqualToOne);
        }

        [Fact]
        public void TryValidate_ShouldReturnTrue_WhenValid()
        {
            var playtime = Playtime.Create(8, 2).Value;

            var isValid = Playtime.TryValidate(playtime, out var errors);

            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }


        [Fact]
        public void TryValidateValue_ShouldReturnTrue_WhenValid()
        {
            var isValid = Playtime.TryValidateValue(5, 2, out var errors);

            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidateValue_ShouldReturnErrors_WhenInvalid()
        {
            var isValid = Playtime.TryValidateValue(-1, 0, out var errors);

            isValid.ShouldBeFalse();
            errors.ShouldContain(Playtime.HoursGreaterThanOrEqualToZero);
            errors.ShouldContain(Playtime.PlayerCountGreaterThanOrEqualToOne);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString_WhenValuesAreProvided()
        {
            var playtime = Playtime.Create(12, 3).Value;

            var str = playtime.ToString();

            str.ShouldBe("12 hours, 3 players");
        }

        [Fact]
        public void ToString_ShouldReturnUnknown_WhenValuesAreNull()
        {
            var playtime = Playtime.Create().Value;

            var str = playtime.ToString();

            str.ShouldBe("Unknown hours, Unknown players");
        }
    }
}
