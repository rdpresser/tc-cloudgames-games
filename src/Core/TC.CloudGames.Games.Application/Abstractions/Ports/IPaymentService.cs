namespace TC.CloudGames.Games.Application.Abstractions.Ports
{
    public interface IPaymentService
    {
        Task<Result<Guid>> ProcessPaymentAsync(Guid userId, Guid gameId, decimal Amount, string paymentMethod);
    }
}
