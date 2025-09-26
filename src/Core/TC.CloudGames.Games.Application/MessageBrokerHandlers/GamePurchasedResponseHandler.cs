namespace TC.CloudGames.Games.Application.MessageBrokerHandlers
{
    public class GamePurchasedResponseHandler
    {
        private readonly IUserGameLibraryRepository _repository;
        private readonly ILogger<GamePurchasedResponseHandler> _logger;

        public GamePurchasedResponseHandler(IUserGameLibraryRepository repository, IMartenOutbox outbox, ILogger<GamePurchasedResponseHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(EventContext<GamePaymentStatusUpdateIntegrationEvent> @event, CancellationToken cancellationToken = default)
        {
            var mapResult = await MapEventToAggregateAsync(@event, cancellationToken);
            if (!mapResult.IsSuccess)
            {
                foreach (var error in mapResult.ValidationErrors)
                {
                    _logger.LogError("Error processing payment status update for UserGameLibrary {UserGameLibraryId}: {ErrorCode} - {ErrorMessage}",
                        @event.EventData.AggregateId, error.ErrorCode, error.ErrorMessage);
                }
                return;
            }

            var aggregate = mapResult.Value;
            await _repository.SaveAsync(aggregate, cancellationToken).ConfigureAwait(false);
            await _repository.CommitAsync(aggregate, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Payment status update processed successfully for UserGameLibrary {UserGameLibraryId}", @event.EventData.AggregateId);
        }

        private async Task<Result<UserGameLibraryAggregate>> MapEventToAggregateAsync(EventContext<GamePaymentStatusUpdateIntegrationEvent> @event, CancellationToken cancellationToken = default)
        {
            var aggregate = await _repository.GetByIdAsync(@event.EventData.AggregateId, cancellationToken);
            if (aggregate == null)
                return Result<UserGameLibraryAggregate>.Invalid(new ValidationError("UserGameLibrary.NotFound", $"UserGameLibrary with ID {@event.EventData.AggregateId} not found."));

            var result = aggregate.UpdateGamePaymentStatus(isApproved: @event.EventData.Success, errorMessage: @event.EventData.ErrorMessage);
            if (!result.IsSuccess)
                return Result<UserGameLibraryAggregate>.Invalid(result.ValidationErrors);

            return Result<UserGameLibraryAggregate>.Success(aggregate);
        }
    }
}
