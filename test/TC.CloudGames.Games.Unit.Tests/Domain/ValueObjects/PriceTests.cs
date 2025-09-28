namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class PriceTests
    {
        [Theory]
        [InlineData(0.01, true)]
        [InlineData(59.99, true)]
        [InlineData(999.99, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(-0.01, false)]
        public void Create_WithValidAmount_ShouldReturnSuccess(decimal amount, bool expectedSuccess)
        {
            // Act
            var result = Price.Create(amount);

            // Assert
            if (expectedSuccess)
            {
                result.IsSuccess.ShouldBeTrue();
                result.Value.Amount.ShouldBe(amount);
            }
            else
            {
                result.IsSuccess.ShouldBeFalse();
                result.ValidationErrors.ShouldNotBeEmpty();
            }
        }

        [Fact]
        public void FromDb_WithValidAmount_ShouldCreateValueObject()
        {
            // Arrange
            var amount = 59.99m;

            // Act
            var result = Price.FromDb(amount);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Amount.ShouldBe(amount);
        }

        [Fact]
        public void TryValidate_WithValidPrice_ShouldReturnTrue()
        {
            // Arrange
            var price = Price.Create(59.99m).Value;

            // Act
            var isValid = Price.TryValidate(price, out var errors);

            // Assert
            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidate_WithInvalidPrice_ShouldReturnFalse()
        {
            // Arrange
            var price = new Price(0); // Invalid price

            // Act
            var isValid = Price.TryValidate(price, out var errors);

            // Assert
            isValid.ShouldBeFalse();
            errors.ShouldNotBeEmpty();
        }
    }
}

