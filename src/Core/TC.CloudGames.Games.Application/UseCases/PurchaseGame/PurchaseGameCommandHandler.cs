using Polly;
using Polly.Retry;
using Wolverine.Runtime.RemoteInvocation;

namespace TC.CloudGames.Games.Application.UseCases.PurchaseGame
{
    internal sealed class PurchaseGameCommandHandler
        : BaseCommandHandler<PurchaseGameCommand, PurchaseGameResponse, UserGameLibraryAggregate, IUserGameLibraryRepository>
    {
        private readonly IGameRepository _gameRepository;
        private readonly IMartenOutbox _outbox;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<PurchaseGameCommandHandler> _logger;

        public PurchaseGameCommandHandler(
            IUserGameLibraryRepository repository,
            IGameRepository gameRepository,
            IUserContext userContext,
            IMartenOutbox outbox,
            IMessageBus messageBus,
            ILogger<PurchaseGameCommandHandler> logger)
            : base(repository, userContext)
        {
            _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
            _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
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

            _logger.LogInformation("Iniciando processo de pagamento para User {UserId}, Game {GameId}, Valor {Amount}", 
                UserContext.Id, command.GameId, game.Price);

            // Configure retry policy for Wolverine message bus calls
            var retryPolicy = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<WolverineRequestReplyException>()
                        .Handle<TimeoutException>()
                        .Handle<InvalidOperationException>(ex => ex.Message.Contains("Unable to determine how to send message")),
                    MaxRetryAttempts = 3,
                    DelayGenerator = static args =>
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber)) + 
                                   TimeSpan.FromMilliseconds(Random.Shared.Next(0, 250)); // jitter
                        return new ValueTask<TimeSpan?>(delay);
                    },
                    OnRetry = args =>
                    {
                        _logger.LogWarning("[Retry {AttemptNumber}] Falha ao chamar Payments. Tentando novamente em {Delay}. Erro: {ErrorMessage}",
                            args.AttemptNumber, args.RetryDelay, args.Outcome.Exception?.Message);
                        return default;
                    }
                })
                .Build();

            // Call the external payment API - Wolverine MessageBroker RPC request/response with retry
            ChargePaymentResponse paymentResult;
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                paymentResult = await retryPolicy.ExecuteAsync(async (cancellationToken) =>
                {
                    _logger.LogDebug("Enviando requisição de pagamento para Payments service");
                    
                    return await _messageBus.InvokeAsync<ChargePaymentResponse>(
                        new ChargePaymentRequest(UserContext.Id, command.GameId, game.Price, command.PaymentMethod.Method),
                        timeout: TimeSpan.FromSeconds(30),
                        cancellation: cancellationToken);
                }, ct);

                stopwatch.Stop();
                _logger.LogInformation("Pagamento processado com sucesso em {ElapsedMs}ms para PaymentId {PaymentId}", 
                    stopwatch.ElapsedMilliseconds, paymentResult.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha definitiva ao processar pagamento após todas as tentativas para User {UserId}, Game {GameId}", 
                    UserContext.Id, command.GameId);
                return Result<UserGameLibraryAggregate>.Invalid(new ValidationError("Payment.ServiceUnavailable", 
                    "O serviço de pagamento está indisponível no momento. Tente novamente mais tarde."));
            }

            if (!paymentResult.Success)
            {
                _logger.LogWarning("Pagamento rejeitado: {ErrorMessage} para User {UserId}, Game {GameId}", 
                    paymentResult.ErrorMessage, UserContext.Id, command.GameId);
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
