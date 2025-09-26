using static TC.CloudGames.Games.Domain.Aggregates.UserGameLibrary.UserGameLibraryAggregate;

namespace TC.CloudGames.Games.Infrastructure.Projections
{
    public class UserGameLibraryProjectionHandler : EventProjection
    {
        public static void Project(UserGameLibraryCreatedDomainEvent @event, IDocumentOperations operations)
        {
            var projection = new UserGameLibraryProjection
            {
                Id = @event.AggregateId,
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

        public static async Task Project(UserGameLibraryGamePaymentStatusUpdateDomainEvent @event, IDocumentOperations operations)
        {
            var projection = await operations.LoadAsync<UserGameLibraryProjection>(@event.AggregateId).ConfigureAwait(false);
            if (projection == null) return;

            projection.Id = @event.AggregateId;
            projection.UserId = @event.UserId;

            // TODO: Consider if we need to update GameId, PaymentId, GameName, Amount, PurchaseDate on payment status update

            projection.UpdatedAt = @event.OccurredOn;
            operations.Store(projection);
        }
    }
}
