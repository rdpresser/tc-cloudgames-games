namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class DiskSizeTests
    {
        [Theory]
        [InlineData(0.1, true)]
        [InlineData(15.5, true)]
        [InlineData(100.0, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(-0.1, false)]
        public void Create_WithValidSize_ShouldReturnSuccess(decimal sizeInGb, bool expectedSuccess)
        {
            // Act
            var result = DiskSize.Create(sizeInGb);

            // Assert
            if (expectedSuccess)
            {
                result.IsSuccess.ShouldBeTrue();
                result.Value.SizeInGb.ShouldBe(sizeInGb);
            }
            else
            {
                result.IsSuccess.ShouldBeFalse();
                result.ValidationErrors.ShouldNotBeEmpty();
            }
        }

        [Fact]
        public void FromDb_WithValidSize_ShouldCreateValueObject()
        {
            // Arrange
            var sizeInGb = 15.5m;

            // Act
            var result = DiskSize.FromDb(sizeInGb);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.SizeInGb.ShouldBe(sizeInGb);
        }

        [Fact]
        public void TryValidate_WithValidDiskSize_ShouldReturnTrue()
        {
            // Arrange
            var diskSize = DiskSize.Create(15.5m).Value;

            // Act
            var isValid = DiskSize.TryValidate(diskSize, out var errors);

            // Assert
            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidate_WithInvalidDiskSize_ShouldReturnFalse()
        {
            // Arrange
            var diskSize = new DiskSize(0); // Invalid disk size

            // Act
            var isValid = DiskSize.TryValidate(diskSize, out var errors);

            // Assert
            isValid.ShouldBeFalse();
            errors.ShouldNotBeEmpty();
        }
    }
}

