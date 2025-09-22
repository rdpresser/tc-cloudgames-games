namespace TC.CloudGames.Games.Application.UseCases.PurchaseGame
{
    /// <summary>
    /// Command to purchase a game for a user.
    /// Contains UserId, GameId and PaymentMethod.
    /// </summary>
    public sealed record PurchaseGameCommand(
        Guid GameId,
        PaymentMethod PaymentMethod)
        : IBaseCommand<PurchaseGameResponse>;

    /// <summary>
    /// Value object representing the payment method.
    /// </summary>
    public sealed record PaymentMethod(
        string Method,
        string? CardLast4Digits = null);
}
