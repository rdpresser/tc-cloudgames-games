namespace TC.CloudGames.Games.Application.UseCases.GetGameById
{
    public class GameByIdResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public DateOnly ReleaseDate { get; init; }
        public string AgeRating { get; init; } = null!;
        public string? Description { get; init; }
        public DeveloperInfo DeveloperInfo { get; set; } = null!;
        public decimal DiskSize { get; init; }
        public decimal Price { get; init; }
        public Playtime? Playtime { get; set; }
        public GameDetails GameDetails { get; set; } = null!;
        public SystemRequirements SystemRequirements { get; set; } = null!;
        public decimal? Rating { get; init; }
        public string? OfficialLink { get; init; }
        public string? GameStatus { get; init; }
    }

    public sealed class DeveloperInfo(string developer, string? publisher)
    {
        public string Developer { get; init; } = developer;
        public string? Publisher { get; init; } = publisher;
    }

    public sealed class Price(decimal amount)
    {
        public decimal Amount { get; init; } = amount;
    }

    public sealed class Playtime(int? hours, int? playerCount)
    {
        public int? Hours { get; init; } = hours;
        public int? PlayerCount { get; init; } = playerCount;
    }

    public sealed class SystemRequirements(string minimum, string? recommended)
    {
        public string Minimum { get; init; } = minimum;
        public string? Recommended { get; init; } = recommended;
    }

    public sealed class GameDetails
    {
        public GameDetails()
        {
            //Marten
        }

        public GameDetails(string? genre, string[] platforms, string? tags, string gameMode, string distributionFormat, string? availableLanguages, bool supportsDlcs)
        {
            Genre = genre;
            Platforms = platforms;
            Tags = tags;
            GameMode = gameMode;
            DistributionFormat = distributionFormat;
            AvailableLanguages = availableLanguages;
            SupportsDlcs = supportsDlcs;
        }

        public string? Genre { get; init; }
        public string? Tags { get; init; }
        public string GameMode { get; init; } = null!;
        public string DistributionFormat { get; init; } = null!;
        public string? AvailableLanguages { get; init; }
        public bool SupportsDlcs { get; init; }
        public string[] Platforms { get; init; } = Array.Empty<string>();
    }
}
