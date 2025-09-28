using TC.CloudGames.Games.Domain.Aggregates.Game;
using TC.CloudGames.Games.Domain.ValueObjects;

using DeveloperInfo = TC.CloudGames.Games.Domain.ValueObjects.DeveloperInfo;
using GameDetails = TC.CloudGames.Games.Domain.ValueObjects.GameDetails;
using Playtime = TC.CloudGames.Games.Domain.ValueObjects.Playtime;
using Price = TC.CloudGames.Games.Domain.ValueObjects.Price;
using SystemRequirements = TC.CloudGames.Games.Domain.ValueObjects.SystemRequirements;


namespace TC.CloudGames.Games.Unit.Tests.Common
{
    public static class TestHelpers
    {
        public static GameAggregate CreateValidGameAggregate()
        {
            var result = GameAggregate.CreateFromPrimitives(
                name: "Test Game",
                releaseDate: DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                ageRating: "E",
                developer: "Test Developer",
                publisher: "Test Publisher",
                diskSizeInGb: 15.5m,
                priceAmount: 59.99m,
                genre: "Action",
                platforms: new[] { "PC", "PlayStation 5", "Xbox Series X" },
                tags: "action,adventure,single-player",
                gameMode: "Single Player",
                distributionFormat: "Digital",
                availableLanguages: "English,Portuguese",
                supportsDlcs: true,
                minimumSystemRequirements: "Windows 10, 8GB RAM, GTX 1060",
                recommendedSystemRequirements: "Windows 11, 16GB RAM, RTX 3070",
                description: "A test game for unit testing",
                playtimeHours: 20,
                playerCount: 1,
                ratingAverage: 8.5m,
                officialLink: "https://example.com/game",
                gameStatus: "Released"
            );

            return result.Value;
        }

        public static AgeRating CreateValidAgeRating() => AgeRating.Create("E").Value;
        public static DeveloperInfo CreateValidDeveloperInfo() => DeveloperInfo.Create("Test Developer", "Test Publisher").Value;

        public static DiskSize CreateValidDiskSize() => DiskSize.Create(15.5m).Value;
        public static Price CreateValidPrice() => Price.Create(59.99m).Value;
        public static GameDetails CreateValidGameDetails() => GameDetails.Create("Action", new[] { "PC" }, "action,adventure", "Single Player", "Digital", "English", true).Value;
        public static SystemRequirements CreateValidSystemRequirements() => SystemRequirements.Create("Windows 10, 8GB RAM", "Windows 11, 16GB RAM").Value;
        public static Playtime CreateValidPlaytime() => Playtime.Create(20, 1).Value;
        public static Rating CreateValidRating() => Rating.Create(8.5m).Value;
    }
}
