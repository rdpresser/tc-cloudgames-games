namespace TC.CloudGames.Games.Application.UseCases.PurchaseGame
{
    public static class PurchaseGameMapper
    {
        public static Result<UserGameLibraryAggregate> ToAggregate(PurchaseGameCommand command, Guid userId, string gameName, decimal amount)
        {
            try
            {
                var aggregate = UserGameLibraryAggregate.Create(
                    userId: userId,
                    gameId: command.GameId,
                    paymentId: Guid.NewGuid(),
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
                UserId: aggregate.UserId,
                GameId: aggregate.GameId,
                PaymentId: aggregate.PaymentId,
                Amount: aggregate.Amount,
                IsApproved: aggregate.IsApproved,
                PurchaseDate: aggregate.PurchaseDate.UtcDateTime
            );


        public static GamePurchasedIntegrationEvent ToIntegrationEvent(UserGameLibraryAggregate.UserGameLibraryCreatedDomainEvent domainEvent)
        => new(
                domainEvent.AggregateId, // UserGameLibraryAggregate Id
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
