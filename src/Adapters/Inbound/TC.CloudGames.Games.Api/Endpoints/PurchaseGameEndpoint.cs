using TC.CloudGames.Games.Application.UseCases.PurchaseGame;

namespace TC.CloudGames.Games.Api.Endpoints
{
    public sealed class PurchaseGameEndpoint : BaseApiEndpoint<PurchaseGameCommand, PurchaseGameResponse>
    {
        public override void Configure()
        {
            Post("game/purchase");
            Roles(AppConstants.AdminRole);
            PostProcessor<LoggingCommandPostProcessorBehavior<PurchaseGameCommand, PurchaseGameResponse>>();
            Description(
                x => x.Produces<PurchaseGameResponse>(201)
                      .ProducesProblemDetails());
            Summary(s =>
            {
                s.Summary = "Endpoint for purchasing a game.";
                s.Description = "This endpoint allows a user to purchase a game by providing the game ID and user ID. Upon successful purchase, a confirmation response is returned.";
                s.ExampleRequest = PurchaseGameCommandExample();
                s.ResponseExamples[201] = PurchaseGameResponseExample();
                s.Responses[201] = "Returned when a game is successfully purchased.";
                s.Responses[400] = "Returned when a bad request occurs.";
                s.Responses[403] = "Returned when the caller lacks the required role to access this endpoint.";
                s.Responses[401] = "Returned when the request is made without a valid user token.";
            });
        }

        public override async Task HandleAsync(PurchaseGameCommand req, CancellationToken ct)
        {
            var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);
            if (response.IsSuccess)
            {
                string location = $"{BaseURL}api/game/purchase/";
                object routeValues = new { id = response.Value.PaymentId };
                await Send.CreatedAtAsync(location, routeValues, response.Value, cancellation: ct).ConfigureAwait(false);
                return;
            }
            await Send.ErrorsAsync(cancellation: ct).ConfigureAwait(false);
        }
        public static PurchaseGameCommand PurchaseGameCommandExample()
        {
            return new PurchaseGameCommand(
                GameId: Guid.NewGuid(),
                PaymentMethod: new("credit_card", "9246"));
        }
        public static PurchaseGameResponse PurchaseGameResponseExample()
        {
            return new PurchaseGameResponse(
                UserId: Guid.NewGuid(),
                GameId: Guid.NewGuid(),
                PaymentId: Guid.NewGuid(),
                Amount: 59.99m,
                IsApproved: false,
                PurchaseDate: DateTime.UtcNow);
        }
    }
}
