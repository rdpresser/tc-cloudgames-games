namespace TC.CloudGames.Games.Infrastructure.Repositories
{
    public class UserGameLibraryRepository : BaseRepository<UserGameLibraryAggregate>, IUserGameLibraryRepository
    {
        public UserGameLibraryRepository(IDocumentSession session)
            : base(session)
        {
        }

        public override Task<IEnumerable<UserGameLibraryAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UserOwnsGameAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default)
        {
            var userGameLibraryProjection = await Session.Query<UserGameLibraryProjection>()
                .Where(x => x.IsActive)
                .Where(x => x.UserId == userId && x.GameId == gameId)
                .FirstOrDefaultAsync(cancellationToken);

            return userGameLibraryProjection != null;
        }
    }
}
