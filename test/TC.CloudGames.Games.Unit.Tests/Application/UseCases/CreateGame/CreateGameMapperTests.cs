using TC.CloudGames.Games.Unit.Tests.Common;
using DeveloperInfo = TC.CloudGames.Games.Application.UseCases.CreateGame.DeveloperInfo;
using GameDetails = TC.CloudGames.Games.Application.UseCases.CreateGame.GameDetails;
using Playtime = TC.CloudGames.Games.Application.UseCases.CreateGame.Playtime;
using SystemRequirements = TC.CloudGames.Games.Application.UseCases.CreateGame.SystemRequirements;

namespace TC.CloudGames.Games.Unit.Tests.Application.UseCases.CreateGame
{
    public class CreateGameMapperTests
    {
        [Fact]
        public void ToAggregate_WithValidCommand_ShouldReturnSuccessResult()
        {
            // Arrange
            var command = new CreateGameCommand(
                Name: "Test Game",
                ReleaseDate: DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                AgeRating: "E",
                Description: "A test game",
                DeveloperInfo: new DeveloperInfo("Test Developer", "Test Publisher"),
                DiskSize: 15.5m,
                Price: 59.99m,
                Playtime: new Playtime(20, 1),
                GameDetails: new GameDetails(
                    Genre: "Action",
                    Platforms: new[] { "PC", "PlayStation 5" },
                    Tags: "action,adventure",
                    GameMode: "Single Player",
                    DistributionFormat: "Digital",
                    AvailableLanguages: "English,Portuguese",
                    SupportsDlcs: true
                ),
                SystemRequirements: new SystemRequirements(
                    Minimum: "Windows 10, 8GB RAM",
                    Recommended: "Windows 11, 16GB RAM"
                ),
                Rating: 8.5m,
                OfficialLink: "https://example.com/game",
                GameStatus: "Released"
            );

            // Act
            var result = CreateGameMapper.ToAggregate(command);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Name.ShouldBe(command.Name);
            result.Value.ReleaseDate.ShouldBe(command.ReleaseDate);
            result.Value.Description.ShouldBe(command.Description);
            result.Value.DeveloperInfo.Developer.ShouldBe(command.DeveloperInfo.Developer);
            result.Value.DeveloperInfo.Publisher.ShouldBe(command.DeveloperInfo.Publisher);
            result.Value.DiskSize.SizeInGb.ShouldBe(command.DiskSize);
            result.Value.Price.Amount.ShouldBe(command.Price);
            result.Value.OfficialLink.ShouldBe(command.OfficialLink);
            result.Value.GameStatus.ShouldBe(command.GameStatus);
        }

        [Fact]
        public void ToAggregate_WithInvalidCommand_ShouldReturnFailureResult()
        {
            // Arrange
            var command = new CreateGameCommand(
                Name: "", // Invalid name
                ReleaseDate: DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                AgeRating: "E",
                Description: "A test game",
                DeveloperInfo: new DeveloperInfo("Test Developer", "Test Publisher"),
                DiskSize: 15.5m,
                Price: 59.99m,
                Playtime: new Playtime(20, 1),
                GameDetails: new GameDetails(
                    Genre: "Action",
                    Platforms: new[] { "PC" },
                    Tags: "action,adventure",
                    GameMode: "Single Player",
                    DistributionFormat: "Digital",
                    AvailableLanguages: "English",
                    SupportsDlcs: true
                ),
                SystemRequirements: new SystemRequirements(
                    Minimum: "Windows 10, 8GB RAM",
                    Recommended: "Windows 11, 16GB RAM"
                ),
                Rating: 8.5m,
                OfficialLink: "https://example.com/game",
                GameStatus: "Released"
            );

            // Act
            var result = CreateGameMapper.ToAggregate(command);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldNotBeEmpty();
            result.ValidationErrors.Any(e => e.Identifier == "Name.Required").ShouldBeTrue();
        }

        [Fact]
        public void FromAggregate_WithValidAggregate_ShouldReturnCorrectResponse()
        {
            // Arrange
            var game = TestHelpers.CreateValidGameAggregate();

            // Act
            var result = CreateGameMapper.FromAggregate(game);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Id.ShouldBe(game.Id);
            result.Value.Name.ShouldBe(game.Name);
            result.Value.ReleaseDate.ShouldBe(game.ReleaseDate);
            result.Value.AgeRating.ShouldBe(game.AgeRating.Value);
            result.Value.Description.ShouldBe(game.Description);
            result.Value.Developer.ShouldBe(game.DeveloperInfo.Developer);
            result.Value.Publisher.ShouldBe(game.DeveloperInfo.Publisher);
            result.Value.DiskSize.ShouldBe(game.DiskSize.SizeInGb);
            result.Value.Price.ShouldBe(game.Price.Amount);
            result.Value.OfficialLink.ShouldBe(game.OfficialLink);
            result.Value.GameStatus.ShouldBe(game.GameStatus);
            result.Value.CreatedAt.ShouldBe(game.CreatedAt);
            result.Value.IsActive.ShouldBe(game.IsActive);
        }

        [Fact]
        public void ToIntegrationEvent_WithValidDomainEvent_ShouldReturnCorrectEvent()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var domainEvent = new GameAggregate.GameCreatedDomainEvent(
                AggregateId: gameId,
                Name: "Test Game",
                ReleaseDate: DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                AgeRating: "E",
                Description: "A test game",
                Developer: "Test Developer",
                Publisher: "Test Publisher",
                DiskSizeInGb: 15.5m,
                PriceAmount: 59.99m,
                Genre: "Action",
                Platforms: new[] { "PC" },
                Tags: "action,adventure",
                GameMode: "Single Player",
                DistributionFormat: "Digital",
                AvailableLanguages: "English",
                SupportsDlcs: true,
                MinimumSystemRequirements: "Windows 10, 8GB RAM",
                RecommendedSystemRequirements: "Windows 11, 16GB RAM",
                PlaytimeHours: 20,
                PlayerCount: 1,
                RatingAverage: 8.5m,
                OfficialLink: "https://example.com/game",
                GameStatus: "Released",
                OccurredOn: DateTimeOffset.UtcNow
            );

            // Act
            var result = CreateGameMapper.ToIntegrationEvent(domainEvent);

            // Assert
            result.ShouldNotBeNull();
            result.AggregateId.ShouldBe(gameId);
            result.Name.ShouldBe(domainEvent.Name);
            result.ReleaseDate.ShouldBe(domainEvent.ReleaseDate);
            result.AgeRating.ShouldBe(domainEvent.AgeRating);
            result.Description.ShouldBe(domainEvent.Description);
            result.Developer.ShouldBe(domainEvent.Developer);
            result.Publisher.ShouldBe(domainEvent.Publisher);
            result.DiskSize.ShouldBe(domainEvent.DiskSizeInGb);
            result.Price.ShouldBe(domainEvent.PriceAmount);
            result.Genre.ShouldBe(domainEvent.Genre);
            result.Platforms.ShouldBe(domainEvent.Platforms);
            result.Tags.ShouldBe(domainEvent.Tags);
            result.GameMode.ShouldBe(domainEvent.GameMode);
            result.DistributionFormat.ShouldBe(domainEvent.DistributionFormat);
            result.AvailableLanguages.ShouldBe(domainEvent.AvailableLanguages);
            result.SupportsDlcs.ShouldBe(domainEvent.SupportsDlcs);
            result.MinimumSystemRequirements.ShouldBe(domainEvent.MinimumSystemRequirements);
            result.RecommendedSystemRequirements.ShouldBe(domainEvent.RecommendedSystemRequirements);
            result.PlaytimeHours.ShouldBe(domainEvent.PlaytimeHours);
            result.PlayerCount.ShouldBe(domainEvent.PlayerCount);
            result.Rating.ShouldBe(domainEvent.RatingAverage);
            result.OfficialLink.ShouldBe(domainEvent.OfficialLink);
            result.GameStatus.ShouldBe(domainEvent.GameStatus);
        }
    }
}

