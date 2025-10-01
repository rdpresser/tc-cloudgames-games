using Shouldly;
using TC.CloudGames.Games.Domain.Aggregates.Game;
using TC.CloudGames.Games.Domain.ValueObjects;

namespace TC.CloudGames.Games.Unit.Tests.Domain.Aggregates.Game
{
    public class GameAggregateTests
    {
        private static GameAggregate BuildValidAggregate()
        {
            var result = GameAggregate.CreateFromPrimitives(
                "My Game",
                new DateOnly(2025, 1, 1),
                "E",
                "Dev Studio",
                "Publisher Co",
                20,
                59.99m,
                "Action",
                new List<string> { "PC" },
                "tags",
                "Singleplayer",
                "Digital",
                "English",
                true,
                "Windows 10, 8GB RAM",
                "Windows 11, 16GB RAM",
                "A test description",
                10,
                1,
                8.5m,
                "https://game.com",
                "Released"
            );
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }

        [Fact]
        public void CreateFromPrimitives_ShouldReturnSuccess_WhenValid()
        {
            var result = GameAggregate.CreateFromPrimitives(
                "Test Game",
                new DateOnly(2025, 1, 1),
                "E",
                "Developer",
                null,
                50,
                99.99m,
                "RPG",
                new List<string> { "PC" },
                "Fantasy, Adventure",
                "Singleplayer",
                "Digital",
                "English",
                true,
                "Windows 10, 16GB RAM",
                null,
                "Epic RPG",
                40,
                1,
                9.5m,
                "https://test.com",
                "Released"
            );

            result.IsSuccess.ShouldBeTrue();
            var aggregate = result.Value;

            aggregate.Name.ShouldBe("Test Game");
            aggregate.Price.Amount.ShouldBe(99.99m);
            aggregate.GameDetails.Genre.ShouldBe("RPG");
            aggregate.SystemRequirements.Minimum.ShouldContain("Windows 10");
            aggregate.AgeRating.Value.ShouldBe("E");
            aggregate.GameStatus.ShouldBe("Released");
        }

        [Fact]
        public void UpdatePrice_ShouldUpdate_WhenValid()
        {
            var game = BuildValidAggregate();

            var result = game.UpdatePrice(79.99m);

            result.IsSuccess.ShouldBeTrue();
            game.Price.Amount.ShouldBe(79.99m);
        }

        [Fact]
        public void UpdatePrice_ShouldFail_WhenInvalid()
        {
            var game = BuildValidAggregate();

            var result = game.UpdatePrice(-1);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(Price.GreaterThanOrEqualToZero);
        }

        [Fact]
        public void UpdateGameStatus_ShouldUpdate_WhenValid()
        {
            var game = BuildValidAggregate();

            var result = game.UpdateGameStatus("Early Access");

            result.IsSuccess.ShouldBeTrue();
            game.GameStatus.ShouldBe("Early Access");
        }

        [Fact]
        public void Activate_ShouldSetActive_WhenNotActive()
        {
            var game = BuildValidAggregate();
            game.Deactivate(); // deixa inativo primeiro

            var result = game.Activate();

            result.IsSuccess.ShouldBeTrue();
            game.IsActive.ShouldBeTrue();
        }

        [Fact]
        public void Deactivate_ShouldSetInactive_WhenActive()
        {
            var game = BuildValidAggregate();

            var result = game.Deactivate();

            result.IsSuccess.ShouldBeTrue();
            game.IsActive.ShouldBeFalse();
        }
    }
}
