using Shouldly;
using TC.CloudGames.Games.Domain.ValueObjects;

namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class SystemRequirementsTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenMinimumIsValid_AndRecommendedIsNull()
        {
            // Act
            var result = SystemRequirements.Create("Windows 10, 8GB RAM");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Minimum.ShouldBe("Windows 10, 8GB RAM");
            result.Value.Recommended.ShouldBeNull();
        }

        [Fact]
        public void Create_ShouldReturnSuccess_WhenMinimumAndRecommendedAreValid()
        {
            // Act
            var result = SystemRequirements.Create(
                "Windows 10, 8GB RAM",
                "Windows 11, 16GB RAM"
            );

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Minimum.ShouldBe("Windows 10, 8GB RAM");
            result.Value.Recommended.ShouldBe("Windows 11, 16GB RAM");
        }

        [Fact]
        public void Create_ShouldReturnError_WhenMinimumIsNullOrEmpty()
        {
            // Act
            var result1 = SystemRequirements.Create("");

            // Assert
            result1.IsSuccess.ShouldBeFalse();
            result1.ValidationErrors.ShouldContain(SystemRequirements.MinimumRequired);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenMinimumExceedsMaxLength()
        {
            // Arrange
            var longText = new string('M', 1001);

            // Act
            var result = SystemRequirements.Create(longText);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(SystemRequirements.MinimumMaximumLength);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenRecommendedExceedsMaxLength()
        {
            // Arrange
            var longText = new string('R', 1001);

            // Act
            var result = SystemRequirements.Create("Windows 10, 8GB RAM", longText);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(SystemRequirements.RecommendedMaximumLength);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenInstanceIsNull()
        {
            // Act
            var result = SystemRequirements.Validate(null);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(SystemRequirements.MinimumRequired);
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenInstanceIsValid()
        {
            var requirements = SystemRequirements.Create("Linux, 4GB RAM").Value;

            var result = SystemRequirements.Validate(requirements);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenValid()
        {
            var requirements = SystemRequirements.Create("MacOS, 8GB RAM").Value;

            var isValid = SystemRequirements.IsValid(requirements);

            isValid.ShouldBeTrue();
        }

        [Fact]
        public void FromDb_ShouldReturnSuccess_WhenMinimumIsValid()
        {
            var result = SystemRequirements.FromDb("Windows 10, 8GB RAM", "Windows 11, 16GB RAM");

            result.IsSuccess.ShouldBeTrue();
            result.Value.Minimum.ShouldBe("Windows 10, 8GB RAM");
            result.Value.Recommended.ShouldBe("Windows 11, 16GB RAM");
        }

        [Fact]
        public void FromDb_ShouldReturnError_WhenMinimumIsNullOrEmpty()
        {
            var result = SystemRequirements.FromDb("");

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(SystemRequirements.MinimumRequired);
        }

        [Fact]
        public void TryValidate_ShouldReturnTrue_WhenValid()
        {
            var requirements = SystemRequirements.Create("Windows 10, 4GB RAM").Value;

            var isValid = SystemRequirements.TryValidate(requirements, out var errors);

            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidate_ShouldReturnErrors_WhenInvalid()
        {
            var requirements = SystemRequirements.Create("", "").Value; // deve ser inválido

            var isValid = SystemRequirements.TryValidate(requirements, out var errors);

            isValid.ShouldBeFalse();
            errors.ShouldContain(SystemRequirements.MinimumRequired);
        }

        [Fact]
        public void TryValidateValue_ShouldReturnTrue_WhenValid()
        {
            var isValid = SystemRequirements.TryValidateValue(
                "Windows 10, 8GB RAM",
                "Windows 11, 16GB RAM",
                out var errors
            );

            isValid.ShouldBeTrue();
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void TryValidateValue_ShouldReturnErrors_WhenInvalid()
        {
            var longText = new string('X', 1001);

            var isValid = SystemRequirements.TryValidateValue("", longText, out var errors);

            isValid.ShouldBeFalse();
            errors.ShouldContain(SystemRequirements.MinimumRequired);
            errors.ShouldContain(SystemRequirements.RecommendedMaximumLength);
        }

        [Fact]
        public void ToString_ShouldReturnMinimum_WhenRecommendedIsNull()
        {
            var requirements = SystemRequirements.Create("Linux, 4GB RAM").Value;

            var str = requirements.ToString();

            str.ShouldBe("Linux, 4GB RAM");
        }

        [Fact]
        public void ToString_ShouldReturnMinimumAndRecommended_WhenBothAreProvided()
        {
            var requirements = SystemRequirements.Create("Windows 10", "Windows 11").Value;

            var str = requirements.ToString();

            str.ShouldBe("Windows 10 (Recommended: Windows 11)");
        }
    }
}
