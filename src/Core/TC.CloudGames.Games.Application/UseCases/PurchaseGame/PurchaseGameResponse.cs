namespace TC.CloudGames.Games.Application.UseCases.PurchaseGame
{
    /// <summary>
    /// Response returned after a successful game purchase.
    /// </summary>
    public sealed record PurchaseGameResponse(
        Guid UserId,
        Guid GameId,
        Guid PaymentId,
        decimal Amount,
        DateTime PurchaseDate);
}
