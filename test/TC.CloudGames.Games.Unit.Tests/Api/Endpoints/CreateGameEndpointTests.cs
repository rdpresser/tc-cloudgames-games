using TC.CloudGames.SharedKernel.Application.Handlers;

namespace TC.CloudGames.Games.Unit.Tests.Api.Endpoints
{
    public class CreateGameEndpointTests
    {
        [Fact]
        public async Task CreateGame_ValidRequest_ReturnsCreated()
        {
            var ep = Factory.Create<CreateGameEndpoint>();
            var req = new CreateGameCommand(
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

            var res = new CreateGameResponse(
                Id: Guid.NewGuid(),
                Name: "Test Game",
                ReleaseDate: DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                AgeRating: "E",
                Description: "A test game",
                Developer: "Test Developer",
                Publisher: "Test Publisher",
                DiskSize: 15.5m,
                Price: 59.99m,
                Genre: "Action",
                Platforms: new[] { "PC", "PlayStation 5" },
                Tags: "action,adventure",
                GameMode: "Single Player",
                DistributionFormat: "Digital",
                AvailableLanguages: "English,Portuguese",
                SupportsDlcs: true,
                MinimumSystemRequirements: "Windows 10, 8GB RAM",
                RecommendedSystemRequirements: "Windows 11, 16GB RAM",
                PlaytimeHours: 20,
                PlayerCount: 1,
                Rating: 8.5m,
                OfficialLink: "https://example.com/game",
                GameStatus: "Released",
                CreatedAt: DateTimeOffset.UtcNow,
                IsActive: true
            );

            var fakeHandler = A.Fake<BaseCommandHandler<CreateGameCommand, CreateGameResponse, GameAggregate, IGameRepository>>();
            A.CallTo(() => fakeHandler.ExecuteAsync(A<CreateGameCommand>.Ignored, A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(Result<CreateGameResponse>.Success(res)));

            fakeHandler.RegisterForTesting();

            await ep.HandleAsync(req, TestContext.Current.CancellationToken);

            // Assert
            ep.Response.Id.ShouldBe(res.Id);
            ep.Response.Name.ShouldBe(res.Name);
            ep.Response.ReleaseDate.ShouldBe(res.ReleaseDate);
            ep.Response.AgeRating.ShouldBe(res.AgeRating);
            ep.Response.Description.ShouldBe(res.Description);
            ep.Response.Developer.ShouldBe(res.Developer);
            ep.Response.Publisher.ShouldBe(res.Publisher);
            ep.Response.DiskSize.ShouldBe(res.DiskSize);
            ep.Response.Price.ShouldBe(res.Price);
            ep.Response.Genre.ShouldBe(res.Genre);
            ep.Response.Platforms.ShouldBe(res.Platforms);
            ep.Response.Tags.ShouldBe(res.Tags);
            ep.Response.GameMode.ShouldBe(res.GameMode);
            ep.Response.DistributionFormat.ShouldBe(res.DistributionFormat);
            ep.Response.AvailableLanguages.ShouldBe(res.AvailableLanguages);
            ep.Response.SupportsDlcs.ShouldBe(res.SupportsDlcs);
            ep.Response.MinimumSystemRequirements.ShouldBe(res.MinimumSystemRequirements);
            ep.Response.RecommendedSystemRequirements.ShouldBe(res.RecommendedSystemRequirements);
            ep.Response.PlaytimeHours.ShouldBe(res.PlaytimeHours);
            ep.Response.PlayerCount.ShouldBe(res.PlayerCount);
            ep.Response.Rating.ShouldBe(res.Rating);
            ep.Response.OfficialLink.ShouldBe(res.OfficialLink);
            ep.Response.GameStatus.ShouldBe(res.GameStatus);
            ep.Response.CreatedAt.ShouldBe(res.CreatedAt);
            ep.Response.IsActive.ShouldBe(res.IsActive);
        }

        [Fact]
        public async Task CreateGame_InvalidRequest_ReturnsBadRequest()
        {
            var ep = Factory.Create<CreateGameEndpoint>();
            var req = new CreateGameCommand(
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

            var listError = new List<ValidationError>
            {
                new() {
                    Identifier = "Name",
                    ErrorMessage = "Game name is required.",
                    ErrorCode = "Name.Required"
                },
                new() {
                    Identifier = "AgeRating",
                    ErrorMessage = "Invalid age rating specified.",
                    ErrorCode = "AgeRating.Invalid"
                }
            };

            var fakeHandler = A.Fake<BaseCommandHandler<CreateGameCommand, CreateGameResponse, GameAggregate, IGameRepository>>();
            A.CallTo(() => fakeHandler.ExecuteAsync(A<CreateGameCommand>.Ignored, A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(Result<CreateGameResponse>.Invalid(listError)));

            fakeHandler.RegisterForTesting();

            await ep.HandleAsync(req, TestContext.Current.CancellationToken);

            // Assert
            ep.Response.ShouldBeNull(); // Should be null for validation errors
        }
    }
}

