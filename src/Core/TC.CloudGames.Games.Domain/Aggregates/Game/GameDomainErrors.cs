using System.Diagnostics.CodeAnalysis;
using TC.CloudGames.Games.Domain.Abstractions;

namespace TC.CloudGames.Games.Domain.Aggregates.Game
{
    [ExcludeFromCodeCoverage]
    public static class GameDomainErrors
    {
        public static readonly DomainError NotFound = new(
            "Game.NotFound",
            "The game with the specified identifier was not found",
            "Game.NotFound");

        public static readonly DomainError JwtSecretKeyNotConfigured = new(
            "JWTSecretKey",
            "JWT secret key is not configured.",
            "JWT.SecretKeyNotConfigured");
    }
}
