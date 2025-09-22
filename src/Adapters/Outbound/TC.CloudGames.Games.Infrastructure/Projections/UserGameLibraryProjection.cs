namespace TC.CloudGames.Games.Infrastructure.Projections
{
    public class UserGameLibraryProjection
    {
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public Guid PaymentId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTimeOffset PurchaseDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
