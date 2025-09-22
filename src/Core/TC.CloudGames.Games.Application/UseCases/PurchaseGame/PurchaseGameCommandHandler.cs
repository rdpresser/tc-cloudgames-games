namespace TC.CloudGames.Games.Application.UseCases.PurchaseGame
{
    internal sealed class PurchaseGameCommandHandler
        : BaseCommandHandler<PurchaseGameCommand, PurchaseGameResponse, UserGameLibraryAggregate, IUserGameLibraryRepository>
    {
        private readonly IGameRepository _gameRepository;
        ////private readonly IPaymentService _paymentService;
        private readonly IMartenOutbox _outbox;
        private readonly ILogger<PurchaseGameCommandHandler> _logger;

        public PurchaseGameCommandHandler(
            IUserGameLibraryRepository repository,
            IGameRepository gameRepository,
            IUserContext userContext,
            ////IPaymentService paymentService,
            IMartenOutbox outbox,
            ILogger<PurchaseGameCommandHandler> logger)
            : base(repository, userContext)
        {
            _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
            ////_paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maps the command to the aggregate.
        /// Calls the payment service and creates the UserGameLibraryAggregate if successful.
        /// </summary>
        protected override async Task<Result<UserGameLibraryAggregate>> MapCommandToAggregateAsync(PurchaseGameCommand command, CancellationToken ct = default)
        {
            // Get from game projection the price of the game
            var game = await _gameRepository.GetGameByIdAsync(command.GameId, ct);
            if (game == null)
            {
                return Result<UserGameLibraryAggregate>.Invalid(new ValidationError("Game.NotFound", $"Game with ID {command.GameId} not found."));
            }

            // Call the external payment API - Wolverine MessageBroker RPC request/response
            ////var paymentResult = await _paymentService.ProcessPaymentAsync(command.UserId, command.GameId, game.Price, command.PaymentMethod.Method);
            var paymentResult = await _outbox.InvokeAsync<ChargePaymentResponse>(
                new ChargePaymentRequest(UserContext.Id, command.GameId, game.Price, command.PaymentMethod.Method),
                timeout: TimeSpan.FromSeconds(10),
                cancellation: ct);

            if (!paymentResult.Success)
            {
                return Result.Invalid(new ValidationError("ChargePaymentRequest.Error", paymentResult.ErrorMessage));
            }

            // Map to aggregate
            var aggregateResult = PurchaseGameMapper.ToAggregate(command, UserContext.Id, paymentResult.PaymentId!.Value, game.Name, game.Price);
            if (!aggregateResult.IsSuccess)
            {
                return Result<UserGameLibraryAggregate>.Invalid(aggregateResult.ValidationErrors);
            }

            return Result<UserGameLibraryAggregate>.Success(aggregateResult.Value);
        }

        private async Task<Result> ValidateAggregateAsync(PurchaseGameCommand command, CancellationToken ct = default)
        {
            // Example: check if the user already owns this game
            if (await Repository.UserOwnsGameAsync(UserContext.Id, command.GameId, ct))
            {
                return Result.Invalid(new ValidationError("UserGameLibrary.AlreadyExists", $"User already owns Game with this ID {command.GameId}."));
            }

            return Result.Success();
        }

        /// <summary>
        /// Publishes integration events (GamePurchasedIntegrationEvent) via Wolverine outbox.
        /// </summary>
        protected override async Task PublishIntegrationEventsAsync(UserGameLibraryAggregate aggregate, CancellationToken ct = default)
        {
            var mappings = new Dictionary<Type, Func<BaseDomainEvent, GamePurchasedIntegrationEvent>>
            {
                { typeof(UserGameLibraryAggregate.UserGameLibraryCreatedDomainEvent), e => PurchaseGameMapper.ToIntegrationEvent((UserGameLibraryAggregate.UserGameLibraryCreatedDomainEvent)e) }
            };

            var integrationEvents = aggregate.UncommittedEvents
                .MapToIntegrationEvents(
                    aggregate: aggregate,
                    userContext: UserContext,
                    handlerName: nameof(PurchaseGameCommandHandler),
                    mappings: mappings
                );

            foreach (var evt in integrationEvents)
            {
                _logger.LogDebug(
                    "Queueing integration event {EventType} for user {UserId}, game {GameId} in Marten outbox",
                    evt.EventData.GetType().Name,
                    evt.AggregateId,
                    evt.EventData.RelatedIds!["GameId"]);

                await _outbox.PublishAsync(evt).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes the command with the following steps:
        /// Map → Validate → Save → Publish → Commit → Respond.
        /// </summary>
        public override async Task<Result<PurchaseGameResponse>> ExecuteAsync(
            PurchaseGameCommand command,
            CancellationToken ct = default)
        {
            // 1. Validate aggregate
            var validationResult = await ValidateAggregateAsync(command, ct).ConfigureAwait(false);
            if (!validationResult.IsSuccess)
            {
                AddErrors(validationResult.ValidationErrors);
                return BuildValidationErrorResult();
            }

            // 2. Map command to aggregate
            var mapResult = await MapCommandToAggregateAsync(command, ct).ConfigureAwait(false);
            if (!mapResult.IsSuccess)
            {
                AddErrors(mapResult.ValidationErrors);
                return BuildValidationErrorResult();
            }

            var aggregate = mapResult.Value;

            // 3. Persist aggregate events
            await Repository.SaveAsync(aggregate, ct).ConfigureAwait(false);

            // 4. Publish integration events
            await PublishIntegrationEventsAsync(aggregate, ct).ConfigureAwait(false);

            // 5. Commit changes
            await Repository.CommitAsync(aggregate, ct).ConfigureAwait(false);

            _logger.LogInformation("Purchase completed successfully for User {UserId}, Game {GameId}", UserContext.Id, command.GameId);

            // 6. Map to response
            return PurchaseGameMapper.FromAggregate(aggregate);
        }
    }
}
