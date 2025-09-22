namespace TC.CloudGames.Games.Domain.Aggregates.UserGameLibrary
{
    /// <summary>
    /// Aggregate root representing a purchased game by a user.
    /// Responsible for maintaining the library of games for each user.
    /// </summary>
    public sealed class UserGameLibraryAggregate : BaseAggregateRoot
    {
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public Guid PaymentId { get; private set; }
        public string GameName { get; private set; } = string.Empty;
        public decimal Amount { get; private set; }
        public DateTimeOffset PurchaseDate { get; private set; } = DateTimeOffset.UtcNow;

        // Parameterless constructor for ORM / Event Sourcing
        public UserGameLibraryAggregate() : base() { }

        // Private constructor for factory methods
        private UserGameLibraryAggregate(Guid id) : base(id) { }

        #region Factory Methods

        /// <summary>
        /// Creates a new UserGameLibraryAggregate with validated primitive values.
        /// </summary>
        public static Result<UserGameLibraryAggregate> Create(Guid userId, Guid gameId, Guid paymentId, string gameName, decimal amount, DateTimeOffset? purchaseDate = null)
        {
            var errors = new List<ValidationError>();

            if (userId == Guid.Empty)
                errors.Add(new ValidationError("UserId.Required", "UserId is required."));

            if (gameId == Guid.Empty)
                errors.Add(new ValidationError("GameId.Required", "GameId is required."));

            if (paymentId == Guid.Empty)
                errors.Add(new ValidationError("PaymentId.Required", "PaymentId is required."));

            if (errors.Any())
                return Result.Invalid(errors.ToArray());

            var aggregate = new UserGameLibraryAggregate(Guid.NewGuid());
            var @event = new UserGameLibraryCreatedDomainEvent(
                aggregate.Id,
                userId,
                gameId,
                paymentId,
                gameName,
                amount,
                purchaseDate ?? DateTimeOffset.UtcNow);

            aggregate.ApplyEvent(@event);
            return Result.Success(aggregate);
        }

        #endregion

        #region Event Application

        private void ApplyEvent(BaseDomainEvent @event)
        {
            AddNewEvent(@event);
            switch (@event)
            {
                case UserGameLibraryCreatedDomainEvent createdEvent:
                    Apply(createdEvent);
                    break;
            }
        }

        public void Apply(UserGameLibraryCreatedDomainEvent @event)
        {
            SetId(@event.AggregateId);
            UserId = @event.UserId;
            GameId = @event.GameId;
            PaymentId = @event.PaymentId;
            PurchaseDate = @event.PurchaseDate;
            GameName = @event.GameName;
            Amount = @event.Amount;
            SetCreatedAt(@event.OccurredOn);
        }

        #endregion

        #region Domain Events

        /// <summary>
        /// Event triggered when a new game is purchased and added to a user's library.
        /// </summary>
        public record UserGameLibraryCreatedDomainEvent(
            Guid AggregateId,
            Guid UserId,
            Guid GameId,
            Guid PaymentId,
            string GameName,
            decimal Amount,
            DateTimeOffset PurchaseDate,
            DateTimeOffset OccurredOn = default) : BaseDomainEvent(AggregateId, OccurredOn == default ? DateTimeOffset.UtcNow : OccurredOn);

        #endregion
    }
}
