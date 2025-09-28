using TC.CloudGames.Games.Api.Endpoints;
using TC.CloudGames.Games.Unit.Tests.Api.Abstractions;
using TC.CloudGames.SharedKernel.Application.Handlers;

namespace TC.CloudGames.Games.Unit.Tests.Api.Endpoints
{
    public class GetGameByIdEndpointTests() : TestBase<App>
    {
        [Fact]
        public async Task GetGameById_ValidId_ReturnsGame()
        {
            var ep = Factory.Create<GetGameByIdEndpoint>();
            var gameId = Guid.NewGuid();
            var req = new GetGameByIdQuery(gameId);

            var res = new GameByIdResponse
            {
                Id = gameId,
                Name = "Test Game",
                ReleaseDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                AgeRating = "E",
                Description = "A test game",
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                DiskSize = 15.5m,
                Price = 59.99m,
                Genre = "Action",
                Platforms = new[] { "PC", "PlayStation 5" },
                Tags = "action,adventure",
                GameMode = "Single Player",
                DistributionFormat = "Digital",
                AvailableLanguages = "English,Portuguese",
                SupportsDlcs = true,
                MinimumSystemRequirements = "Windows 10, 8GB RAM",
                RecommendedSystemRequirements = "Windows 11, 16GB RAM",
                PlaytimeHours = 20,
                PlayerCount = 1,
                Rating = 8.5m,
                OfficialLink = "https://example.com/game",
                GameStatus = "Released",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };

            var fakeHandler = A.Fake<BaseQueryHandler<GetGameByIdQuery, GameByIdResponse>>();
            A.CallTo(() => fakeHandler.ExecuteAsync(A<GetGameByIdQuery>.Ignored, A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(Result<GameByIdResponse>.Success(res)));

            fakeHandler.RegisterForTesting();

            await ep.HandleAsync(req, TestContext.Current.CancellationToken);

            // Assert
            ep.Response.ShouldNotBeNull();
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
            ep.Response.UpdatedAt.ShouldBe(res.UpdatedAt);
            ep.Response.IsActive.ShouldBe(res.IsActive);
        }

        [Fact]
        public async Task GetGameById_NonExistentId_ReturnsNotFound()
        {
            var ep = Factory.Create<GetGameByIdEndpoint>();
            var gameId = Guid.NewGuid();
            var req = new GetGameByIdQuery(gameId);

            var fakeHandler = A.Fake<BaseQueryHandler<GetGameByIdQuery, GameByIdResponse>>();
            A.CallTo(() => fakeHandler.ExecuteAsync(A<GetGameByIdQuery>.Ignored, A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(Result<GameByIdResponse>.Success(null)));

            fakeHandler.RegisterForTesting();

            await ep.HandleAsync(req, TestContext.Current.CancellationToken);

            // Assert
            ep.Response.ShouldBeNull();
        }

        [Fact]
        public async Task GetGameById_InvalidId_ReturnsBadRequest()
        {
            var ep = Factory.Create<GetGameByIdEndpoint>();
            var req = new GetGameByIdQuery(Guid.Empty); // Invalid ID

            var listError = new List<ValidationError>
            {
                new() {
                    Identifier = "GameId",
                    ErrorMessage = "Game ID is required.",
                    ErrorCode = "GameId.Required"
                }
            };

            var fakeHandler = A.Fake<BaseQueryHandler<GetGameByIdQuery, GameByIdResponse>>();
            A.CallTo(() => fakeHandler.ExecuteAsync(A<GetGameByIdQuery>.Ignored, A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(Result<GameByIdResponse>.Invalid(listError)));

            fakeHandler.RegisterForTesting();

            await ep.HandleAsync(req, TestContext.Current.CancellationToken);

            // Assert
            ep.Response.ShouldBeNull();
        }
    }
}

