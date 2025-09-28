using TC.CloudGames.Games.Unit.Tests.Common;
using Xunit;

namespace TC.CloudGames.Games.Unit.Tests.Domain.Aggregates
{
    public class GameAggregateTests
    {
        [Fact]
        public void CreateFromResult_WithAllSuccessResults_ShouldCreateGameAggregate()
        {
            // Arrange
            var name = "Test Game";
            var releaseDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var ageRatingResult = AgeRating.Create("E");
            var developerInfoResult = DeveloperInfo.Create("Test Developer", "Test Publisher");
            var diskSizeResult = DiskSize.Create(15.5m);
            var priceResult = Price.Create(59.99m);
            var gameDetailsResult = GameDetails.Create("Action", new[] { "PC" }, "action,adventure", "Single Player", "Digital", "English", true);
            var systemRequirementsResult = SystemRequirements.Create("Windows 10, 8GB RAM", "Windows 11, 16GB RAM");

            // Act
            var result = GameAggregate.CreateFromResult(name, releaseDate, ageRatingResult, developerInfoResult, diskSizeResult, priceResult, gameDetailsResult, systemRequirementsResult);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Name.ShouldBe(name);
            result.Value.ReleaseDate.ShouldBe(releaseDate);
            result.Value.AgeRating.Value.ShouldBe("E");
            result.Value.DeveloperInfo.Developer.ShouldBe("Test Developer");
            result.Value.Price.Amount.ShouldBe(59.99m);
        }

        [Fact]
        public void CreateFromResult_WithAnyFailure_ShouldReturnFailure()
        {
            // Arrange
            var name = "Test Game";
            var releaseDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var ageRatingResult = AgeRating.Create(""); // Invalid
            var developerInfoResult = DeveloperInfo.Create("Test Developer", "Test Publisher");
            var diskSizeResult = DiskSize.Create(15.5m);
            var priceResult = Price.Create(59.99m);
            var gameDetailsResult = GameDetails.Create("Action", new[] { "PC" }, "action,adventure", "Single Player", "Digital", "English", true);
            var systemRequirementsResult = SystemRequirements.Create("Windows 10, 8GB RAM", "Windows 11, 16GB RAM");

            // Act
            var result = GameAggregate.CreateFromResult(name, releaseDate, ageRatingResult, developerInfoResult, diskSizeResult, priceResult, gameDetailsResult, systemRequirementsResult);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldNotBeEmpty();
        }

        [Fact]
        public void CreateFromPrimitives_WithValidData_ShouldCreateGameAggregate()
        {
            // Arrange
            var name = "Test Game";
            var releaseDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var ageRating = "E";
            var developer = "Test Developer";
            var publisher = "Test Publisher";
            var diskSizeInGb = 15.5m;
            var priceAmount = 59.99m;
            var genre = "Action";
            var platforms = new[] { "PC", "PlayStation 5" };
            var tags = "action,adventure";
            var gameMode = "Single Player";
            var distributionFormat = "Digital";
            var availableLanguages = "English,Portuguese";
            var supportsDlcs = true;
            var minimumSystemRequirements = "Windows 10, 8GB RAM";
            var recommendedSystemRequirements = "Windows 11, 16GB RAM";

            // Act
            var result = GameAggregate.CreateFromPrimitives(
                name, releaseDate, ageRating, developer, publisher, diskSizeInGb, priceAmount,
                genre, platforms, tags, gameMode, distributionFormat, availableLanguages, supportsDlcs,
                minimumSystemRequirements, recommendedSystemRequirements);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Name.ShouldBe(name);
            result.Value.ReleaseDate.ShouldBe(releaseDate);
            result.Value.AgeRating.Value.ShouldBe(ageRating);
            result.Value.DeveloperInfo.Developer.ShouldBe(developer);
            result.Value.Price.Amount.ShouldBe(priceAmount);
        }

        [Fact]
        public void CreateFromPrimitives_WithInvalidName_ShouldReturnFailure()
        {
            // Arrange
            var name = "";
            var releaseDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

            // Act
            var result = GameAggregate.CreateFromPrimitives(
                name, releaseDate, "E", "Test Developer", "Test Publisher", 15.5m, 59.99m,
                "Action", new[] { "PC" }, "action,adventure", "Single Player", "Digital", "English", true,
                "Windows 10, 8GB RAM", "Windows 11, 16GB RAM");

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldNotBeEmpty();
            result.ValidationErrors.Any(e => e.Identifier.StartsWith(expectedErrorPrefix)).ShouldBeTrue();
        }
    }
}
