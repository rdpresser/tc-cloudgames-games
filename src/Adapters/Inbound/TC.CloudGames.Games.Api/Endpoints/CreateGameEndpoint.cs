using TC.CloudGames.Games.Infrastructure.Elasticsearch;

namespace TC.CloudGames.Games.Api.Endpoints
{
    public sealed class CreateGameEndpoint : BaseApiEndpoint<CreateGameCommand, CreateGameResponse>
    {
        private readonly IGameSearchService _searchService;

        public CreateGameEndpoint(IGameSearchService searchService)
        {
            _searchService = searchService;
        }

        public override void Configure()
        {
            Post("game");
            Roles(AppConstants.AdminRole);
            PostProcessor<LoggingCommandPostProcessorBehavior<CreateGameCommand, CreateGameResponse>>();

            Description(
                x => x.Produces<CreateGameResponse>(201)
                      .ProducesProblemDetails());

            Summary(s =>
            {
                s.Summary = "Endpoint for creating a new game.";
                s.Description = "This endpoint allows for the creation of a new game by providing its name, description, and other relevant details. Upon successful creation, a new game is added to the system.";
                s.ExampleRequest = CreateGameCommandExample();
                s.ResponseExamples[201] = CreateGameResponseExample();
                s.Responses[201] = "Returned when a new game is successfully created.";
                s.Responses[400] = "Returned when a bad request occurs.";
                s.Responses[403] = "Returned when the caller lacks the required role to access this endpoint.";
                s.Responses[401] = "Returned when the request is made without a valid user token.";
            });
        }
        public override async Task HandleAsync(CreateGameCommand req, CancellationToken ct)
        {
            var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                var projection = MapToProjection(response.Value);
                await _searchService.IndexAsync(projection, ct);

                string location = $"{BaseURL}api/game/";
                object routeValues = new { id = response.Value.Id };
                await Send.CreatedAtAsync(location, routeValues, response.Value, cancellation: ct).ConfigureAwait(false);
                return;
            }

            await Send.ErrorsAsync(cancellation: ct).ConfigureAwait(false);
        }

        public static CreateGameCommand CreateGameCommandExample()
        {
            return new CreateGameCommand(
                Name: "Game Name",
                ReleaseDate: DateOnly.FromDateTime(DateTime.UtcNow),
                AgeRating: $"Choose one of valid age rate: {AgeRating.ValidRatings.JoinWithQuotes()}",
                Description: "Game Description",
                DeveloperInfo: new CreateGame.DeveloperInfo("Developer Name", "Publisher Name"),
                DiskSize: 50.0m,
                Price: 59.99m,
                Playtime: new CreateGame.Playtime(10, 1),
                GameDetails: new CreateGame.GameDetails
                (
                    Genre: "Genre",
                    Platforms: [.. Domain.ValueObjects.GameDetails.ValidPlatforms],
                    Tags: "Tags",
                    GameMode: $"Choose one of valid game modes: {Domain.ValueObjects.GameDetails.ValidGameModes.JoinWithQuotes()}",
                    DistributionFormat: $"Choose one of valid distribution format: {Domain.ValueObjects.GameDetails.ValidDistributionFormats.JoinWithQuotes()}",
                    AvailableLanguages: "Available Languages",
                    SupportsDlcs: true
                ),
                SystemRequirements: new CreateGame.SystemRequirements("Minimum Requirements", "Recommended Requirements"),
                Rating: 4.5m,
                OfficialLink: "https://example.com",
                GameStatus: $"Choose one of valid game status: {GameAggregate.ValidGameStatus.JoinWithQuotes()}"
            );
        }

        public static CreateGameResponse CreateGameResponseExample()
        {
            return new CreateGameResponse(
                Id: Guid.NewGuid(),
                Name: "Game Name",
                ReleaseDate: DateOnly.FromDateTime(DateTime.UtcNow),
                AgeRating: AgeRating.ValidRatings.First(),
                Description: "Game Description",
                DeveloperInfo: new CreateGame.DeveloperInfo("Developer Name", "Publisher Name"),
                DiskSize: 50.0m,
                Price: 59.99m,
                Playtime: new CreateGame.Playtime(10, 1),
                GameDetails: new CreateGame.GameDetails(
                    Genre: "Genre",
                    Platforms: [.. Domain.ValueObjects.GameDetails.ValidPlatforms],
                    Tags: "Tags",
                    GameMode: Domain.ValueObjects.GameDetails.ValidGameModes.First(),
                    DistributionFormat: Domain.ValueObjects.GameDetails.ValidDistributionFormats.First(),
                    AvailableLanguages: "Available Languages",
                    SupportsDlcs: true),
                SystemRequirements: new CreateGame.SystemRequirements("Minimum Requirements", "Recommended Requirements"),
                Rating: 4.5m,
                OfficialLink: "https://example.com",
                GameStatus: GameAggregate.ValidGameStatus.First()
            );
        }

        private static GameProjection MapToProjection(CreateGameResponse response) => new()
        {
            Id = response.Id,
            Name = response.Name,
            Description = response.Description,
            ReleaseDate = response.ReleaseDate,
            AgeRating = response.AgeRating,
            Developer = response.DeveloperInfo.Developer,
            Publisher = response.DeveloperInfo.Publisher,
            PriceAmount = response.Price,
            RatingAverage = response.Rating,
            Genre = response.GameDetails.Genre,
            Platforms = response.GameDetails.Platforms,
            Tags = response.GameDetails.Tags,
            GameMode = response.GameDetails.GameMode,
            DistributionFormat = response.GameDetails.DistributionFormat,
            AvailableLanguages = response.GameDetails.AvailableLanguages,
            SupportsDlcs = response.GameDetails.SupportsDlcs,
            GameStatus = response.GameStatus
        };
    }
}
