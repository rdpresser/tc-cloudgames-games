using static TC.CloudGames.Games.Domain.Aggregates.UserGameLibrary.UserGameLibraryAggregate;

namespace TC.CloudGames.Games.Infrastructure.Projections
{
    public class UserGameLibraryProjectionHandler : EventProjection
    {
        public static void Project(UserGameLibraryCreatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = new UserGameLibraryProjection
            {
                UserId = @event.UserId,
                GameId = @event.GameId,
                PaymentId = @event.PaymentId,
                GameName = @event.GameName,
                Amount = @event.Amount,
                PurchaseDate = @event.PurchaseDate,
                CreatedAt = @event.OccurredOn,
                UpdatedAt = null,
                IsActive = true
            };
            operations.Store(projection);
        }
    }
}
