namespace TC.CloudGames.Games.Application.UseCases.GetGameById
{
    public sealed record GetGameByIdQuery(Guid Id) : ICachedQuery<GameByIdResponse>
    {
        private string? _cacheKey;
        public string GetCacheKey
        {
            get => _cacheKey ?? $"GetGameByIdQuery-{Id}";
        }

        public TimeSpan? Duration => null;
        public TimeSpan? DistributedCacheDuration => null;

        public void SetCacheKey(string cacheKey)
        {
            _cacheKey = $"GetGameByIdQuery-{Id}-{cacheKey}";
        }
    }
}
