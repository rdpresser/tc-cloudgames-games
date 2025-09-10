using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.CloudGames.Games.Infrastructure.Projections
{
    /// <summary>
    /// Read model projection for Game aggregate optimized for queries.
    /// This class represents the current state of a game, built from domain events.
    /// </summary>
    public class GameProjection
    {
        public Guid Id { get; set; }

        // Basic Information
        public string Name { get; set; } = string.Empty;
        public DateOnly ReleaseDate { get; set; }
        public string AgeRating { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Developer and Publisher Information
        public string Developer { get; set; } = string.Empty;
        public string? Publisher { get; set; }

        // Technical Information
        public decimal DiskSizeInGb { get; set; }
        public decimal PriceAmount { get; set; }

        // Game Details
        public string? Genre { get; set; }
        public string PlatformListJson { get; set; } = "[]";
        public string? Tags { get; set; }
        public string GameMode { get; set; } = string.Empty;
        public string DistributionFormat { get; set; } = string.Empty;
        public string? AvailableLanguages { get; set; }
        public bool SupportsDlcs { get; set; }

        // System Requirements
        public string MinimumSystemRequirements { get; set; } = string.Empty;
        public string? RecommendedSystemRequirements { get; set; }

        // Optional Information
        public int? PlaytimeHours { get; set; }
        public int? PlayerCount { get; set; }
        public decimal? RatingAverage { get; set; }
        public string? OfficialLink { get; set; }
        public string? GameStatus { get; set; }

        // Aggregate Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
