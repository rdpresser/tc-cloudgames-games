namespace TC.CloudGames.Games.Application.UseCases.PurchaseGame
{
    public static class PurchaseGameMapper
    {
        public static Result<UserGameLibraryAggregate> ToAggregate(PurchaseGameCommand command, Guid paymentId, string gameName, decimal amount)
        {
            try
            {
                var aggregate = UserGameLibraryAggregate.Create(
                    userId: command.UserId,
                    gameId: command.GameId,
                    paymentId: paymentId,
                    gameName: gameName,
                    amount: amount
                );

                return Result<UserGameLibraryAggregate>.Success(aggregate);
            }
            catch (Exception ex)
            {
                return Result<UserGameLibraryAggregate>.Invalid(new List<ValidationError>
                {
                    new("UserGameLibraryAggregate.CreateFailed", ex.Message)
                });
            }
        }

        public static PurchaseGameResponse FromAggregate(UserGameLibraryAggregate aggregate)
        => new(
                aggregate.UserId,
                aggregate.GameId,
                aggregate.PaymentId,
                aggregate.Amount,
                aggregate.PurchaseDate.UtcDateTime
            );


        public static GamePurchasedIntegrationEvent ToIntegrationEvent(UserGameLibraryAggregate.UserGameLibraryCreatedDomainEvent domainEvent)
        => new(
                domainEvent.UserId,
                domainEvent.GameId,
                domainEvent.PaymentId,
                domainEvent.GameName,
                domainEvent.Amount,
                DateTime.UtcNow,
                domainEvent.OccurredOn
            );
    }
}
