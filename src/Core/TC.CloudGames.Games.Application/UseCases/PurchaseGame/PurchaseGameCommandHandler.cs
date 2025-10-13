namespace TC.CloudGames.Games.Application.UseCases.PurchaseGame
{
    internal sealed class PurchaseGameCommandHandler
        : BaseCommandHandler<PurchaseGameCommand, PurchaseGameResponse, UserGameLibraryAggregate, IUserGameLibraryRepository>
    {
        private readonly IGameRepository _gameRepository;
        private readonly IMartenOutbox _outbox;
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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maps the command to the aggregate.
        /// Calls the payment service and creates the UserGameLibraryAggregate if successful.
        /// </summary>
        protected override async Task<Result<UserGameLibraryAggregate>> MapCommandToAggregateAsync(PurchaseGameCommand command, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Starting MapCommandToAggregateAsync for User {UserId}, Game {GameId}, PaymentMethod {PaymentMethod}",
                    UserContext.Id, command.GameId, command.PaymentMethod);

                // Get from game projection the price of the game
                var game = await _gameRepository.GetGameByIdAsync(command.GameId, ct);
                if (game == null)
                {
                    _logger.LogWarning("Game not found during mapping: GameId {GameId}, User {UserId}",
                        command.GameId, UserContext.Id);
                    return Result<UserGameLibraryAggregate>.Invalid(new ValidationError("Game.NotFound", $"Game with ID {command.GameId} not found."));
                }

                _logger.LogDebug("Game found for mapping: GameId {GameId}, GameName {GameName}, Price {Price}",
                    command.GameId, game.Name, game.Price);

                // Map to aggregate
                var aggregateResult = PurchaseGameMapper.ToAggregate(command, UserContext.Id, game.Name, game.Price);
                if (!aggregateResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to map command to aggregate: User {UserId}, Game {GameId}, Errors: {Errors}",
                        UserContext.Id, command.GameId, string.Join(", ", aggregateResult.ValidationErrors.Select(e => e.ErrorMessage)));
                    return Result<UserGameLibraryAggregate>.Invalid(aggregateResult.ValidationErrors);
                }

                _logger.LogDebug("Successfully mapped command to aggregate for User {UserId}, Game {GameId}",
                    UserContext.Id, command.GameId);

                return Result<UserGameLibraryAggregate>.Success(aggregateResult.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MapCommandToAggregateAsync: User {UserId}, Game {GameId}, PaymentMethod {PaymentMethod}. Exception: {ExceptionType}",
                    UserContext.Id, command.GameId, command.PaymentMethod, ex.GetType().Name);
                
                // Re-throw with contextual information for SonarQube compliance
                throw new InvalidOperationException($"Failed to map purchase command for User {UserContext.Id}, Game {command.GameId}", ex);
            }
        }

        private async Task<Result> ValidateAggregateAsync(PurchaseGameCommand command, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Starting ValidateAggregateAsync for User {UserId}, Game {GameId}",
                    UserContext.Id, command.GameId);

                // Example: check if the user already owns this game
                var userOwnsGame = await Repository.UserOwnsGameAsync(UserContext.Id, command.GameId, ct);
                
                _logger.LogDebug("User ownership check result: User {UserId}, Game {GameId}, AlreadyOwns: {AlreadyOwns}",
                    UserContext.Id, command.GameId, userOwnsGame);

                if (userOwnsGame)
                {
                    _logger.LogWarning("User already owns game: User {UserId}, Game {GameId}",
                        UserContext.Id, command.GameId);
                    return Result.Invalid(new ValidationError("UserGameLibrary.AlreadyExists", $"User already owns Game with this ID {command.GameId}."));
                }

                _logger.LogDebug("Validation passed for User {UserId}, Game {GameId}",
                    UserContext.Id, command.GameId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ValidateAggregateAsync: User {UserId}, Game {GameId}. Exception: {ExceptionType}",
                    UserContext.Id, command.GameId, ex.GetType().Name);
                
                // Re-throw with contextual information for SonarQube compliance
                throw new InvalidOperationException($"Failed to validate purchase for User {UserContext.Id}, Game {command.GameId}", ex);
            }
        }

        /// <summary>
        /// Publishes integration events (GamePurchasedIntegrationEvent) via Wolverine outbox.
        /// </summary>
        protected override async Task PublishIntegrationEventsAsync(UserGameLibraryAggregate aggregate, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Starting PublishIntegrationEventsAsync for User {UserId}, Game {GameId}, Events count: {EventsCount}",
                    aggregate.UserId, aggregate.GameId, aggregate.UncommittedEvents.Count);

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

                _logger.LogDebug("Mapped {EventsCount} integration events for User {UserId}, Game {GameId}",
                    integrationEvents.Count(), aggregate.UserId, aggregate.GameId);

                foreach (var evt in integrationEvents)
                {
                    _logger.LogDebug(
                        "Queueing integration event {EventType} for user {UserId}, game {GameId} in Marten outbox",
                        evt.EventData.GetType().Name,
                        evt.AggregateId,
                        evt.EventData.RelatedIds!["GameId"]);

                    await _outbox.PublishAsync(evt).ConfigureAwait(false);
                    
                    _logger.LogDebug("Successfully queued integration event {EventType} for user {UserId}, game {GameId}",
                        evt.EventData.GetType().Name, evt.AggregateId, evt.EventData.RelatedIds!["GameId"]);
                }

                _logger.LogInformation("Successfully published {EventsCount} integration events for User {UserId}, Game {GameId}",
                    integrationEvents.Count(), aggregate.UserId, aggregate.GameId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PublishIntegrationEventsAsync: User {UserId}, Game {GameId}, AggregateId {AggregateId}. Exception: {ExceptionType}",
                    aggregate.UserId, aggregate.GameId, aggregate.Id, ex.GetType().Name);
                
                // Re-throw with contextual information for SonarQube compliance
                throw new InvalidOperationException($"Failed to publish integration events for User {aggregate.UserId}, Game {aggregate.GameId}, Aggregate {aggregate.Id}", ex);
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
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("🚀 Starting PurchaseGame execution: User {UserId}, Game {GameId}, PaymentMethod {PaymentMethod}",
                UserContext.Id, command.GameId, command.PaymentMethod);

            try
            {
                // 1. Validate aggregate
                _logger.LogDebug("⏳ Step 1/6: Starting aggregate validation...");
                var validationResult = await ValidateAggregateAsync(command, ct).ConfigureAwait(false);
                if (!validationResult.IsSuccess)
                {
                    _logger.LogWarning("❌ Step 1/6: Validation failed for User {UserId}, Game {GameId}. Errors: {Errors}",
                        UserContext.Id, command.GameId, string.Join(", ", validationResult.ValidationErrors.Select(e => e.ErrorMessage)));
                    AddErrors(validationResult.ValidationErrors);
                    return BuildValidationErrorResult();
                }
                _logger.LogDebug("✅ Step 1/6: Validation completed successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                // 2. Map command to aggregate
                _logger.LogDebug("⏳ Step 2/6: Starting command mapping...");
                var mapResult = await MapCommandToAggregateAsync(command, ct).ConfigureAwait(false);
                if (!mapResult.IsSuccess)
                {
                    _logger.LogWarning("❌ Step 2/6: Mapping failed for User {UserId}, Game {GameId}. Errors: {Errors}",
                        UserContext.Id, command.GameId, string.Join(", ", mapResult.ValidationErrors.Select(e => e.ErrorMessage)));
                    AddErrors(mapResult.ValidationErrors);
                    return BuildValidationErrorResult();
                }
                var aggregate = mapResult.Value;
                _logger.LogDebug("✅ Step 2/6: Mapping completed successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                // 3. Persist aggregate events
                _logger.LogDebug("⏳ Step 3/6: Starting aggregate persistence...");
                try
                {
                    await Repository.SaveAsync(aggregate, ct).ConfigureAwait(false);
                    _logger.LogDebug("✅ Step 3/6: Persistence completed successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Step 3/6: Failed to persist aggregate for User {UserId}, Game {GameId}, AggregateId {AggregateId}. Exception: {ExceptionType}",
                        UserContext.Id, command.GameId, aggregate.Id, ex.GetType().Name);
                    
                    // Re-throw with contextual information for SonarQube compliance
                    throw new InvalidOperationException($"Failed to persist aggregate for User {UserContext.Id}, Game {command.GameId}, Aggregate {aggregate.Id}", ex);
                }

                // 4. Publish integration events
                _logger.LogDebug("⏳ Step 4/6: Starting integration events publishing...");
                try
                {
                    await PublishIntegrationEventsAsync(aggregate, ct).ConfigureAwait(false);
                    _logger.LogDebug("✅ Step 4/6: Integration events published successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Step 4/6: Failed to publish integration events for User {UserId}, Game {GameId}, AggregateId {AggregateId}. Exception: {ExceptionType}",
                        UserContext.Id, command.GameId, aggregate.Id, ex.GetType().Name);
                    
                    // Re-throw with contextual information for SonarQube compliance
                    throw new InvalidOperationException($"Failed to publish integration events for User {UserContext.Id}, Game {command.GameId}, Aggregate {aggregate.Id}", ex);
                }

                // 5. Commit changes
                _logger.LogDebug("⏳ Step 5/6: Starting transaction commit...");
                try
                {
                    await Repository.CommitAsync(aggregate, ct).ConfigureAwait(false);
                    _logger.LogDebug("✅ Step 5/6: Transaction committed successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Step 5/6: Failed to commit transaction for User {UserId}, Game {GameId}, AggregateId {AggregateId}. Exception: {ExceptionType}",
                        UserContext.Id, command.GameId, aggregate.Id, ex.GetType().Name);
                    
                    // Re-throw with contextual information for SonarQube compliance
                    throw new InvalidOperationException($"Failed to commit transaction for User {UserContext.Id}, Game {command.GameId}, Aggregate {aggregate.Id}", ex);
                }

                // 6. Map to response
                _logger.LogDebug("⏳ Step 6/6: Starting response mapping...");
                var response = PurchaseGameMapper.FromAggregate(aggregate);
                _logger.LogDebug("✅ Step 6/6: Response mapping completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                stopwatch.Stop();
                _logger.LogInformation("🎉 Purchase completed successfully for User {UserId}, Game {GameId}, PaymentMethod {PaymentMethod}. Total time: {TotalMs}ms",
                    UserContext.Id, command.GameId, command.PaymentMethod, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (OperationCanceledException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex, "⏸️ Purchase operation was cancelled for User {UserId}, Game {GameId}, PaymentMethod {PaymentMethod} after {ElapsedMs}ms",
                    UserContext.Id, command.GameId, command.PaymentMethod, stopwatch.ElapsedMilliseconds);
                throw new OperationCanceledException($"Purchase operation was cancelled for User {UserContext.Id}, Game {command.GameId}", ex);
            }
            catch (TimeoutException ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "⏰ Purchase operation timed out for User {UserId}, Game {GameId}, PaymentMethod {PaymentMethod} after {ElapsedMs}ms",
                    UserContext.Id, command.GameId, command.PaymentMethod, stopwatch.ElapsedMilliseconds);
                throw new TimeoutException($"Purchase operation timed out for User {UserContext.Id}, Game {command.GameId} after {stopwatch.ElapsedMilliseconds}ms", ex);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "💥 Unexpected error in PurchaseGame execution for User {UserId}, Game {GameId}, PaymentMethod {PaymentMethod} after {ElapsedMs}ms. Exception: {ExceptionType}",
                    UserContext.Id, command.GameId, command.PaymentMethod, stopwatch.ElapsedMilliseconds, ex.GetType().Name);
                throw new InvalidOperationException($"Unexpected error in PurchaseGame execution for User {UserContext.Id}, Game {command.GameId} after {stopwatch.ElapsedMilliseconds}ms", ex);
            }
        }
    }
}
