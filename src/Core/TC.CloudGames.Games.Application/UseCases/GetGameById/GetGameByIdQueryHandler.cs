namespace TC.CloudGames.Games.Application.UseCases.GetGameById
{
    internal sealed class GetGameByIdQueryHandler : BaseQueryHandler<GetGameByIdQuery, GameByIdResponse>
    {
        private readonly IGameRepository _repository;

        public GetGameByIdQueryHandler(IGameRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public override async Task<Result<GameByIdResponse>> ExecuteAsync(GetGameByIdQuery command, CancellationToken ct = default)
        {
            var result = await _repository.GetGameByIdAsync(command.Id, ct).ConfigureAwait(false);
            if (result is not null)
                return result;

            AddError(x => x.Id, $"Game with id '{command.Id}' not found.", GameDomainErrors.NotFound.ErrorCode);
            return BuildNotFoundResult();
        }
    }
}
