namespace TC.CloudGames.Games.Application.UseCases.GetGameList
{
    internal sealed class GetGameListQueryHandler : BaseQueryHandler<GetGameListQuery, IReadOnlyList<GameListResponse>>
    {
        private readonly IGameRepository _repository;

        public GetGameListQueryHandler(IGameRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public override async Task<Result<IReadOnlyList<GameListResponse>>> ExecuteAsync(GetGameListQuery query,
            CancellationToken ct = default)
        {
            var games = await _repository.GetGameListAsync(query, ct).ConfigureAwait(false);

            if (games is null || !games.Any())
                return Result<IReadOnlyList<GameListResponse>>.Success([]);

            return Result.Success<IReadOnlyList<GameListResponse>>([.. games]);
        }
    }
}
