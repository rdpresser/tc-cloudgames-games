using TC.CloudGames.Games.Unit.Tests.Common;
using Xunit;

namespace TC.CloudGames.Games.Unit.Tests.Application.UseCases.GetGameList
{
    public class GameListResponseTests
    {
        [Fact]
        public void GameListResponse_ShouldInheritFromGameByIdResponse()
        {
            // Arrange & Act
            var response = new GameListResponse();

            // Assert
            response.ShouldBeAssignableTo<GameByIdResponse>();
        }

        [Fact]
        public void GameListResponse_ShouldHaveAllInheritedProperties()
        {
            // Arrange
            var game = TestHelpers.CreateValidGameAggregate();
            var developerInfo = new DeveloperInfo(game.DeveloperInfo.Developer, game.DeveloperInfo.Publisher);
            var playtime = new Playtime(game.Playtime?.Hours, game.Playtime?.PlayerCount);
            var gameDetails = new GameDetails(
                game.GameDetails.Genre,
                game.GameDetails.Platforms.ToArray(),
                game.GameDetails.Tags,
                game.GameDetails.GameMode,
                game.GameDetails.DistributionFormat,
                game.GameDetails.AvailableLanguages,
                game.GameDetails.SupportsDlcs
            );
            var systemRequirements = new SystemRequirements(
                game.SystemRequirements.Minimum,
                game.SystemRequirements.Recommended
            );

            // Act
            var response = new GameListResponse
            {
                Id = game.Id,
                Name = game.Name,
                ReleaseDate = game.ReleaseDate,
                AgeRating = game.AgeRating.Value,
                Description = game.Description,
                DeveloperInfo = developerInfo,
                DiskSize = game.DiskSize.SizeInGb,
                Price = game.Price.Amount,
                Playtime = playtime,
                GameDetails = gameDetails,
                SystemRequirements = systemRequirements,
                Rating = game.Rating?.Average,
                OfficialLink = game.OfficialLink,
                GameStatus = game.GameStatus
            };

            // Assert - All inherited properties should be accessible
            response.Id.ShouldBe(game.Id);
            response.Name.ShouldBe(game.Name);
            response.ReleaseDate.ShouldBe(game.ReleaseDate);
            response.AgeRating.ShouldBe(game.AgeRating.Value);
            response.Description.ShouldBe(game.Description);
            response.DeveloperInfo.ShouldNotBeNull();
            response.DeveloperInfo.Developer.ShouldBe(game.DeveloperInfo.Developer);
            response.DeveloperInfo.Publisher.ShouldBe(game.DeveloperInfo.Publisher);
            response.DiskSize.ShouldBe(game.DiskSize.SizeInGb);
            response.Price.ShouldBe(game.Price.Amount);
            response.Playtime.ShouldNotBeNull();
            response.Playtime.Hours.ShouldBe(game.Playtime?.Hours);
            response.Playtime.PlayerCount.ShouldBe(game.Playtime?.PlayerCount);
            response.GameDetails.ShouldNotBeNull();
            response.GameDetails.Genre.ShouldBe(game.GameDetails.Genre);
            response.GameDetails.Platforms.ShouldBe(game.GameDetails.Platforms.ToArray());
            response.GameDetails.Tags.ShouldBe(game.GameDetails.Tags);
            response.GameDetails.GameMode.ShouldBe(game.GameDetails.GameMode);
            response.GameDetails.DistributionFormat.ShouldBe(game.GameDetails.DistributionFormat);
            response.GameDetails.AvailableLanguages.ShouldBe(game.GameDetails.AvailableLanguages);
            response.GameDetails.SupportsDlcs.ShouldBe(game.GameDetails.SupportsDlcs);
            response.SystemRequirements.ShouldNotBeNull();
            response.SystemRequirements.Minimum.ShouldBe(game.SystemRequirements.Minimum);
            response.SystemRequirements.Recommended.ShouldBe(game.SystemRequirements.Recommended);
            response.Rating.ShouldBe(game.Rating?.Average);
            response.OfficialLink.ShouldBe(game.OfficialLink);
            response.GameStatus.ShouldBe(game.GameStatus);
        }

        [Fact]
        public void GameListResponse_CanBeUsedInList()
        {
            // Arrange
            var responses = new List<GameListResponse>
            {
                new() { Id = Guid.NewGuid(), Name = "Game 1", AgeRating = "E", DiskSize = 10.5m, Price = 59.99m },
                new() { Id = Guid.NewGuid(), Name = "Game 2", AgeRating = "T", DiskSize = 15.2m, Price = 79.99m },
                new() { Id = Guid.NewGuid(), Name = "Game 3", AgeRating = "M", DiskSize = 20.0m, Price = 99.99m }
            };

            // Act
            var count = responses.Count;
            var firstGame = responses.First();

            // Assert
            count.ShouldBe(3);
            firstGame.ShouldNotBeNull();
            firstGame.Name.ShouldBe("Game 1");
            firstGame.AgeRating.ShouldBe("E");
            firstGame.DiskSize.ShouldBe(10.5m);
            firstGame.Price.ShouldBe(59.99m);
        }
    }
}

