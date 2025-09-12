﻿using TC.CloudGames.Games.Application.UseCases.GetGameById;

namespace TC.CloudGames.Games.Api.Endpoints
{
    public sealed class GetGameByIdEndpoint : BaseApiEndpoint<GetGameByIdQuery, GameByIdResponse>
    {
        public override void Configure()
        {
            Get("game/{Id}");
            Roles(AppConstants.AdminRole);
            PreProcessor<QueryCachingPreProcessorBehavior<GetGameByIdQuery, GameByIdResponse>>();
            PostProcessor<QueryCachingPostProcessorBehavior<GetGameByIdQuery, GameByIdResponse>>();

            Description(
                x => x.Produces<GameByIdResponse>(200)
                      .ProducesProblemDetails()
                      .Produces((int)HttpStatusCode.NotFound)
                      .Produces((int)HttpStatusCode.Forbidden)
                      .Produces((int)HttpStatusCode.Unauthorized));

            Summary(s =>
            {
                s.Summary = "Retrieve game details by its unique identifier.";
                s.Description = "This endpoint retrieves detailed information about a game by its unique Id. Access is restricted to users with the appropriate roles.";
                s.ExampleRequest = new GetGameByIdQuery(Guid.NewGuid());
                s.ResponseExamples[200] = GetGameResponseExample();
                s.Responses[200] = "Returned when game information is successfully retrieved.";
                s.Responses[400] = "Returned when the request is invalid.";
                s.Responses[404] = "Returned when no game is found with the provided Id.";
                s.Responses[403] = "Returned when the caller lacks the required role to access this endpoint.";
                s.Responses[401] = "Returned when the request is made without a valid user token.";
            });
        }

        public override async Task HandleAsync(GetGameByIdQuery req, CancellationToken ct)
        {
            var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);

            // Use the MatchResultAsync method from the base class
            await MatchResultAsync(response, ct).ConfigureAwait(false);
        }

        public static GameByIdResponse GetGameResponseExample()
        {
            return new GameByIdResponse
            {
                Id = Guid.NewGuid(),
                Name = "Game Name",
                ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow),
                AgeRating = AgeRating.ValidRatings.First(),
                Description = "Game Description",
                DeveloperInfo = new(developer: "Developer Name", publisher: "Publisher Name"),
                DiskSize = 50.0m,
                Price = 59.99m,
                Playtime = new(10, 1),
                GameDetails = new(
                    genre: "Genre",
                    platforms: [.. GameDetails.ValidPlatforms],
                    tags: "Tags",
                    gameMode: GameDetails.ValidGameModes.First(),
                    distributionFormat: GameDetails.ValidDistributionFormats.First(),
                    availableLanguages: "Available Languages",
                    supportsDlcs: true
                ),
                SystemRequirements = new
                (
                    minimum: "Minimum Requirements",
                    recommended: "Recommended Requirements"
                ),
                Rating = 4.5m,
                OfficialLink = "https://example.com",
                GameStatus = GameAggregate.ValidGameStatus.First()
            };
        }
    }
}
