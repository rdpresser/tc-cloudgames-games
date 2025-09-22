namespace TC.CloudGames.Games.Application.UseCases.CreateGame
{
    internal sealed class CreateGameCommandHandler
        : BaseCommandHandler<CreateGameCommand, CreateGameResponse, GameAggregate, IGameRepository>
    {
        private readonly IMartenOutbox _outbox;
        private readonly ILogger<CreateGameCommandHandler> _logger;

        public CreateGameCommandHandler(
            IGameRepository repository,
            IUserContext userContext,
            IMartenOutbox outbox,
            ILogger<CreateGameCommandHandler> logger)
            : base(repository, userContext)
        {
            _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maps the command to the aggregate.
        /// </summary>
        protected override Task<Result<GameAggregate>> MapCommandToAggregateAsync(CreateGameCommand command, CancellationToken ct = default)
        {
            var aggregateResult = CreateGameMapper.ToAggregate(command);
            if (!aggregateResult.IsSuccess)
                return Task.FromResult(Result<GameAggregate>.Invalid(aggregateResult.ValidationErrors));

            return Task.FromResult(Result<GameAggregate>.Success(aggregateResult.Value));
        }

        /// <summary>
        /// Validates the aggregate.
        /// Example: cross-entity checks, uniqueness rules, or custom domain invariants.
        /// </summary>
        protected override Task<Result> ValidateAggregateAsync(GameAggregate aggregate, CancellationToken ct = default)
        {
            // For now, no extra validation beyond the aggregate factory
            // Validate game uniqueness here if needed (Future enhancement)
            return Task.FromResult(Result.Success());
        }

        /// <summary>
        /// Publishes integration events through Wolverine Outbox.
        /// Maps domain events -> integration events and wraps them in EventContext.
        /// </summary>
        protected override async Task PublishIntegrationEventsAsync(GameAggregate aggregate, CancellationToken ct = default)
        {
            var mappings = new Dictionary<Type, Func<BaseDomainEvent, GameCreatedIntegrationEvent>>
            {
                { typeof(GameAggregate.GameCreatedDomainEvent), e => CreateGameMapper.ToIntegrationEvent((GameAggregate.GameCreatedDomainEvent)e) }
            };

            var integrationEvents = aggregate.UncommittedEvents
                .MapToIntegrationEvents(
                    aggregate: aggregate, // vamos resolver esse ponto já já
                    userContext: UserContext,
                    handlerName: nameof(CreateGameCommandHandler),
                    mappings: mappings
                );

            foreach (var evt in integrationEvents)
            {
                _logger.LogDebug(
                    "Queueing integration event {EventType} for game {GameId} in Marten outbox",
                    evt.EventData.GetType().Name,
                    evt.AggregateId);

                await _outbox.PublishAsync(evt).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Main command execution.
        /// Uses the base template (map → validate → save → publish → commit).
        /// </summary>
        public override async Task<Result<CreateGameResponse>> ExecuteAsync(
            CreateGameCommand command,
            CancellationToken ct = default)
        {
            // 1. Map command -> aggregate
            var mapResult = await MapCommandToAggregateAsync(command, ct);
            if (!mapResult.IsSuccess)
            {
                AddErrors(mapResult.ValidationErrors);
                return Result<CreateGameResponse>.Invalid(mapResult.ValidationErrors);
            }

            var aggregate = mapResult.Value;

            // 2. Validate aggregate (optional custom rules)
            var validationResult = await ValidateAggregateAsync(aggregate, ct);
            if (!validationResult.IsSuccess)
            {
                AddErrors(validationResult.ValidationErrors);
                return Result<CreateGameResponse>.Invalid(validationResult.ValidationErrors);
            }

            // 3. Persist aggregate events (event sourcing)
            await Repository.SaveAsync(aggregate, ct);

            // 4. Publish integration events via outbox
            await PublishIntegrationEventsAsync(aggregate);

            // 5. Commit session (persist + flush outbox atomically)
            await Repository.CommitAsync(aggregate, ct);

            _logger.LogInformation("Game {GameId} created successfully and events committed", aggregate.Id);

            // 6. Map response
            return CreateGameMapper.FromAggregate(aggregate);
        }
    }
}
