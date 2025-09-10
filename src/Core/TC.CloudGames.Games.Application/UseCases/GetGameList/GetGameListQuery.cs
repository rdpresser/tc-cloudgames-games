namespace TC.CloudGames.Games.Application.UseCases.GetGameList
{
    public sealed record GetGameListQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string SortBy = "id",
        string SortDirection = "asc",
        string Filter = ""
    ) : ICachedQuery<IReadOnlyList<GameListResponse>>
    {
        private string? _cacheKey;
        public string GetCacheKey
        {
            get => _cacheKey ?? $"GetGameListQuery-{PageNumber}-{PageSize}-{SortBy}-{SortDirection}-{Filter}";
        }

        public TimeSpan? Duration => null;
        public TimeSpan? DistributedCacheDuration => null;

        public void SetCacheKey(string cacheKey)
        {
            _cacheKey = $"GetGameListQuery-{PageNumber}-{PageSize}-{SortBy}-{SortDirection}-{Filter}-{cacheKey}";
        }
    }
}
