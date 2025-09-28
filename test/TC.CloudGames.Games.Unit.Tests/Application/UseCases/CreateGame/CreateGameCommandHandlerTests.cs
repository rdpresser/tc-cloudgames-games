using TC.CloudGames.Games.Unit.Tests.Shared;
using Wolverine.Marten;

namespace TC.CloudGames.Games.Unit.Tests.Application.UseCases.CreateGame
{
    public class CreateGameCommandHandlerTests : BaseTest
    {
        [Fact]
        public async Task ExecuteAsync_WithValidCommand_ShouldReturnSuccessResponse()
        {
            // Arrange
            Factory.RegisterTestServices(_ => { });

            var repo = A.Fake<IGameRepository>();
            var outbox = A.Fake<IMartenOutbox>();
            var logger = A.Fake<ILogger<CreateGameCommandHandler>>();
            var userContext = A.Fake<IUserContext>();

            // Setup user context
            A.CallTo(() => userContext.Id).Returns(Guid.NewGuid());
            A.CallTo(() => userContext.Name).Returns("Test User");
            A.CallTo(() => userContext.Email).Returns("test@example.com");
            A.CallTo(() => userContext.Username).Returns("testuser");
            A.CallTo(() => userContext.Role).Returns("Admin");
            A.CallTo(() => userContext.IsAuthenticated).Returns(true);

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

            var handler = new CreateGameCommandHandler(repo, userContext, outbox, logger);

            // Act
            var result = await handler.ExecuteAsync(command, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Name.ShouldBe(command.Name);
            result.Value.Developer.ShouldBe(command.DeveloperInfo.Developer);
            result.Value.Price.ShouldBe(command.Price);

            // Verify interactions - handler calls SaveAsync and CommitAsync
            A.CallTo(() => repo.SaveAsync(A<GameAggregate>.That.IsNotNull(), A<CancellationToken>.That.IsEqualTo(CancellationToken.None)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => repo.CommitAsync(A<GameAggregate>.That.IsNotNull(), A<CancellationToken>.That.IsEqualTo(CancellationToken.None)))
                .MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("", "Test Game", "Name.Required")]
        [InlineData("Valid Name", "", "AgeRating.Required")]
        [InlineData("Valid Name", "E", "Developer.Required")]
        public async Task ExecuteAsync_WithInvalidCommand_ShouldReturnInvalidResult(
            string name, string ageRating, string expectedErrorPrefix)
        {
            // Arrange
            Factory.RegisterTestServices(_ => { });

            var repo = A.Fake<IGameRepository>();
            var outbox = A.Fake<IMartenOutbox>();
            var logger = A.Fake<ILogger<CreateGameCommandHandler>>();
            var userContext = A.Fake<IUserContext>();

            // Setup user context
            A.CallTo(() => userContext.Id).Returns(Guid.NewGuid());
            A.CallTo(() => userContext.Name).Returns("Test User");
            A.CallTo(() => userContext.Email).Returns("test@example.com");
            A.CallTo(() => userContext.Username).Returns("testuser");
            A.CallTo(() => userContext.Role).Returns("Admin");
            A.CallTo(() => userContext.IsAuthenticated).Returns(true);

            var command = new CreateGameCommand(
                Name: name,
                ReleaseDate: DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                AgeRating: ageRating,
                Description: "A test game",
                DeveloperInfo: new DeveloperInfo(string.IsNullOrEmpty(ageRating) ? "" : "Test Developer", "Test Publisher"),
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

            var handler = new CreateGameCommandHandler(repo, userContext, outbox, logger);

            // Act
            var result = await handler.ExecuteAsync(command, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldNotBeEmpty();
            result.ValidationErrors.Any(e => e.Identifier.StartsWith(expectedErrorPrefix)).ShouldBeTrue();

            // Verify no persistence happened for invalid commands
            A.CallTo(repo).Where(call => call.Method.Name == "SaveAsync").MustNotHaveHappened();
            A.CallTo(repo).Where(call => call.Method.Name == "CommitAsync").MustNotHaveHappened();
            A.CallTo(outbox).Where(call => call.Method.Name == "PublishAsync").MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_WithValidCommand_ShouldPublishIntegrationEvents()
        {
            // Arrange
            Factory.RegisterTestServices(_ => { });

            var repo = A.Fake<IGameRepository>();
            var outbox = A.Fake<IMartenOutbox>();
            var logger = A.Fake<ILogger<CreateGameCommandHandler>>();
            var userContext = A.Fake<IUserContext>();

            // Setup user context
            A.CallTo(() => userContext.Id).Returns(Guid.NewGuid());
            A.CallTo(() => userContext.Name).Returns("Test User");
            A.CallTo(() => userContext.Email).Returns("test@example.com");
            A.CallTo(() => userContext.Username).Returns("testuser");
            A.CallTo(() => userContext.Role).Returns("Admin");
            A.CallTo(() => userContext.IsAuthenticated).Returns(true);

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

            var handler = new CreateGameCommandHandler(repo, userContext, outbox, logger);

            // Act
            var result = await handler.ExecuteAsync(command, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();

            // Verify integration events were published
            A.CallTo(() => outbox.PublishAsync(A<EventContext>.That.IsNotNull()))
                .MustHaveHappenedOnceExactly();
        }
    }
}

