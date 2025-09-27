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
            projection.GameId = @event.GameId;
            projection.PaymentId = @event.PaymentId;
            projection.IsApproved = @event.IsApproved;
            projection.ErrorMessage = @event.ErrorMessage;
            projection.UpdatedAt = @event.OccurredOn;
            operations.Store(projection);
        }
    }
}
