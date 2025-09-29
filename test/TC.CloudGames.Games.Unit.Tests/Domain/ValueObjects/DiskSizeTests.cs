using Shouldly;
using TC.CloudGames.Games.Domain.ValueObjects;

namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class DiskSizeTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenValueIsPositive()
        {
            // Act
            var result = DiskSize.Create(50);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.SizeInGb.ShouldBe(50);
        }

        [Theory]
        [InlineData(null)]
        public void Create_ShouldReturnError_WhenValueIsNullOrZero(decimal? value)
        {
            // Act
            var result = DiskSize.Create(value ?? 0);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(DiskSize.Required);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenValueIsNegative()
        {
            // Act
            var result = DiskSize.Create(-10);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(DiskSize.GreaterThanZero);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenInstanceIsNull()
        {
            // Act
            var result = DiskSize.Validate(null);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(DiskSize.Required);
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenInstanceIsValid()
        {
            // Arrange
            var diskSize = DiskSize.Create(100).Value;

            // Act
            var result = DiskSize.Validate(diskSize);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenInstanceIsValid()
        {
            // Arrange
            var diskSize = DiskSize.Create(20).Value;

            // Act
            var isValid = DiskSize.IsValid(diskSize);

            // Assert
            isValid.ShouldBeTrue();
        }

        [Fact]
        public void FromDb_ShouldReturnSuccess_WhenValueIsPositive()
        {
            // Act
            var result = DiskSize.FromDb(15);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.SizeInGb.ShouldBe(15);
        }

        [Fact]
        public void FromDb_ShouldReturnError_WhenValueIsZeroOrNegative()
        {
            // Act
            var resultZero = DiskSize.FromDb(0);
            var resultNegative = DiskSize.FromDb(-5);

            // Assert
            resultZero.IsSuccess.ShouldBeFalse();
            resultZero.ValidationErrors.ShouldContain(DiskSize.GreaterThanZero);

            resultNegative.IsSuccess.ShouldBeFalse();
            resultNegative.ValidationErrors.ShouldContain(DiskSize.GreaterThanZero);
        }

        [Fact]
        public void TryValidate_ShouldReturnTrue_WhenInstanceIsValid()
        {
            // Arrange
            var diskSize = DiskSize.Create(25).Value;

            // Act
            var isValid = DiskSize.TryValidate(diskSize, out var errors);

            // Assert
            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidate_ShouldReturnErrors_WhenInstanceIsInvalid()
        {
            // Act
            var isValid = DiskSize.TryValidate(null, out var errors);

            // Assert
            isValid.ShouldBeFalse();
            errors.ShouldContain(DiskSize.Required);
        }

        [Fact]
        public void TryValidateValue_ShouldReturnTrue_WhenValueIsValid()
        {
            // Act
            var isValid = DiskSize.TryValidateValue(10, out var errors);

            // Assert
            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void TryValidateValue_ShouldReturnError_WhenValueIsInvalid(decimal value)
        {
            // Act
            var isValid = DiskSize.TryValidateValue(value, out var errors);

            // Assert
            isValid.ShouldBeFalse();
            errors.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void ImplicitConversion_ShouldReturnDecimalValue()
        {
            // Arrange
            var diskSize = DiskSize.Create(30).Value;

            // Act
            decimal value = diskSize;

            // Assert
            value.ShouldBe(30);
        }
    }
}
