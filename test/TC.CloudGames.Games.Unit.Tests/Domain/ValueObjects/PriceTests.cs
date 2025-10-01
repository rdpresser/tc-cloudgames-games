using Shouldly;
using TC.CloudGames.Games.Domain.ValueObjects;

namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class PriceTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenAmountIsZero()
        {
            // Act
            var result = Price.Create(0);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Amount.ShouldBe(0);
        }

        [Fact]
        public void Create_ShouldReturnSuccess_WhenAmountIsPositive()
        {
            // Act
            var result = Price.Create(150.75m);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Amount.ShouldBe(150.75m);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenAmountIsNegative()
        {
            // Act
            var result = Price.Create(-10m);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(Price.GreaterThanOrEqualToZero);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenInstanceIsNull()
        {
            // Act
            var result = Price.Validate(null);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(Price.Required);
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenInstanceIsValid()
        {
            var price = Price.Create(20).Value;

            var result = Price.Validate(price);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenPriceIsValid()
        {
            var price = Price.Create(5).Value;

            var isValid = Price.IsValid(price);

            isValid.ShouldBeTrue();
        }

        [Fact]
        public void FromDb_ShouldReturnSuccess_WhenAmountIsPositive()
        {
            var result = Price.FromDb(300);

            result.IsSuccess.ShouldBeTrue();
            result.Value.Amount.ShouldBe(300);
        }

        [Fact]
        public void FromDb_ShouldReturnError_WhenAmountIsNegative()
        {
            var result = Price.FromDb(-1);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(Price.GreaterThanOrEqualToZero);
        }

        [Fact]
        public void TryValidate_ShouldReturnTrue_WhenPriceIsValid()
        {
            var price = Price.Create(50).Value;

            var isValid = Price.TryValidate(price, out var errors);

            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidateValue_ShouldReturnTrue_WhenAmountIsValid()
        {
            var isValid = Price.TryValidateValue(20, out var errors);

            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidateValue_ShouldReturnErrors_WhenAmountIsNegative()
        {
            var isValid = Price.TryValidateValue(-100, out var errors);

            isValid.ShouldBeFalse();
            errors.ShouldContain(Price.GreaterThanOrEqualToZero);
        }

        [Fact]
        public void ImplicitConversion_ShouldReturnDecimalValue()
        {
            var price = Price.Create(75).Value;

            decimal value = price;

            value.ShouldBe(75);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedCurrency()
        {
            var price = Price.Create(99.99m).Value;

            var str = price.ToString();

            str.ShouldBe(99.99m.ToString("C"));
        }
    }
}
