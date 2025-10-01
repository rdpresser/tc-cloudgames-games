using Shouldly;
using TC.CloudGames.Games.Domain.ValueObjects;

namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class DeveloperInfoTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDeveloperIsValid_AndPublisherIsNull()
        {
            // Act
            var result = DeveloperInfo.Create("Valid Developer");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Developer.ShouldBe("Valid Developer");
            result.Value.Publisher.ShouldBeNull();
        }

        [Fact]
        public void Create_ShouldReturnSuccess_WhenDeveloperAndPublisherAreValid()
        {
            // Act
            var result = DeveloperInfo.Create("Valid Developer", "Valid Publisher");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Developer.ShouldBe("Valid Developer");
            result.Value.Publisher.ShouldBe("Valid Publisher");
        }

        [Fact]
        public void Create_ShouldReturnError_WhenDeveloperIsNullOrEmpty()
        {
            // Act
            var result1 = DeveloperInfo.Create("");

            // Assert
            result1.IsSuccess.ShouldBeFalse();
            result1.ValidationErrors.ShouldContain(DeveloperInfo.DeveloperRequired);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenDeveloperExceedsMaxLength()
        {
            // Arrange
            var longDev = new string('D', 101);

            // Act
            var result = DeveloperInfo.Create(longDev);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(DeveloperInfo.DeveloperMaximumLength);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenPublisherExceedsMaxLength()
        {
            // Arrange
            var longPublisher = new string('P', 201);

            // Act
            var result = DeveloperInfo.Create("Valid Developer", longPublisher);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(DeveloperInfo.PublisherMaximumLength);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenInstanceIsNull()
        {
            // Act
            var result = DeveloperInfo.Validate(null);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(DeveloperInfo.DeveloperRequired);
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenInstanceIsValid()
        {
            // Arrange
            var devInfo = DeveloperInfo.Create("Valid Dev", "Publisher").Value;

            // Act
            var isValid = DeveloperInfo.IsValid(devInfo);

            // Assert
            isValid.ShouldBeTrue();
        }

        [Fact]
        public void FromDb_ShouldReturnSuccess_WhenDeveloperIsValid()
        {
            // Act
            var result = DeveloperInfo.FromDb("Db Developer", "Db Publisher");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Developer.ShouldBe("Db Developer");
            result.Value.Publisher.ShouldBe("Db Publisher");
        }

        [Fact]
        public void FromDb_ShouldReturnError_WhenDeveloperIsNullOrEmpty()
        {
            // Act
            var result = DeveloperInfo.FromDb("");

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(DeveloperInfo.DeveloperRequired);
        }

        [Fact]
        public void TryValidate_ShouldReturnErrors_WhenInvalid()
        {
            // Arrange
            var devInfo = DeveloperInfo.Create("", "Publisher").Value; // deve ser null

            // Act
            var isValid = DeveloperInfo.TryValidate(devInfo, out var errors);

            // Assert
            isValid.ShouldBeFalse();
            errors.ShouldContain(DeveloperInfo.DeveloperRequired);
        }

        [Fact]
        public void TryValidateValue_ShouldReturnTrue_WhenValid()
        {
            // Act
            var isValid = DeveloperInfo.TryValidateValue("Dev", "Publisher", out var errors);

            // Assert
            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidateValue_ShouldReturnError_WhenPublisherTooLong()
        {
            // Arrange
            var longPublisher = new string('P', 201);

            // Act
            var isValid = DeveloperInfo.TryValidateValue("Dev", longPublisher, out var errors);

            // Assert
            isValid.ShouldBeFalse();
            errors.ShouldContain(DeveloperInfo.PublisherMaximumLength);
        }
    }
}
