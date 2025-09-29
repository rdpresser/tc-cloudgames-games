using Shouldly;
using TC.CloudGames.Games.Domain.Aggregates.Game;

namespace TC.CloudGames.Games.Unit.Tests.Domain.Aggregates.Game
{
    public class GameDomainErrorsTests
    {
        [Fact]
        public void NotFound_ShouldHaveExpectedValues()
        {
            // Act
            var error = GameDomainErrors.NotFound;

            // Assert
            error.Property.ShouldBe("Game.NotFound");
            error.ErrorMessage.ShouldBe("The game with the specified identifier was not found");
            error.ErrorCode.ShouldBe("Game.NotFound");
        }

        [Fact]
        public void JwtSecretKeyNotConfigured_ShouldHaveExpectedValues()
        {
            // Act
            var error = GameDomainErrors.JwtSecretKeyNotConfigured;

            // Assert
            error.Property.ShouldBe("JWTSecretKey");
            error.ErrorMessage.ShouldBe("JWT secret key is not configured.");
            error.ErrorCode.ShouldBe("JWT.SecretKeyNotConfigured");
        }
    }
}
