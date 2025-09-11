namespace TC.CloudGames.Games.Infrastructure.Projections
{
    /// <summary>
    /// Read model projection for Game aggregate optimized for queries.
    /// Represents the current state of a game, built from domain events.
    /// </summary>
    public class GameProjection
    {
        public Guid Id { get; set; }

        // -------------------------
        // Basic Information
        // -------------------------
        /// <summary>
        /// The name of the game.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The release date of the game.
        /// </summary>
        public DateOnly ReleaseDate { get; set; }

        /// <summary>
        /// Age rating of the game (e.g., "E", "T", "M").
        /// </summary>
        public string AgeRating { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the game.
        /// </summary>
        public string? Description { get; set; }

        // -------------------------
        // Developer and Publisher Information
        // -------------------------
        /// <summary>
        /// Name of the developer.
        /// </summary>
        public string Developer { get; set; } = string.Empty;

        /// <summary>
        /// Optional publisher of the game.
        /// </summary>
        public string? Publisher { get; set; }

        // -------------------------
        // Technical Information
        // -------------------------
        /// <summary>
        /// Disk size required by the game in GB.
        /// </summary>
        public decimal DiskSizeInGb { get; set; }

        /// <summary>
        /// Price of the game.
        /// </summary>
        public decimal PriceAmount { get; set; }

        // -------------------------
        // Game Details
        // -------------------------
        /// <summary>
        /// Optional genre of the game.
        /// </summary>
        public string? Genre { get; set; }

        /// <summary>
        /// List of platforms the game is available on.
        /// </summary>
        public string[] Platforms { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Optional tags associated with the game.
        /// </summary>
        public string? Tags { get; set; }

        /// <summary>
        /// Game mode (e.g., Single Player, Multiplayer).
        /// </summary>
        public string GameMode { get; set; } = string.Empty;

        /// <summary>
        /// Distribution format (e.g., Digital, Physical).
        /// </summary>
        public string DistributionFormat { get; set; } = string.Empty;

        /// <summary>
        /// Optional available languages in the game.
        /// </summary>
        public string? AvailableLanguages { get; set; }

        /// <summary>
        /// Indicates whether the game supports DLCs.
        /// </summary>
        public bool SupportsDlcs { get; set; }

        // -------------------------
        // System Requirements
        // -------------------------
        /// <summary>
        /// Minimum system requirements to run the game.
        /// </summary>
        public string MinimumSystemRequirements { get; set; } = string.Empty;

        /// <summary>
        /// Optional recommended system requirements for optimal performance.
        /// </summary>
        public string? RecommendedSystemRequirements { get; set; }

        // -------------------------
        // Optional Information
        // -------------------------
        /// <summary>
        /// Optional total playtime hours.
        /// </summary>
        public int? PlaytimeHours { get; set; }

        /// <summary>
        /// Optional maximum player count.
        /// </summary>
        public int? PlayerCount { get; set; }

        /// <summary>
        /// Optional average rating of the game.
        /// </summary>
        public decimal? RatingAverage { get; set; }

        /// <summary>
        /// Optional official link to the game.
        /// </summary>
        public string? OfficialLink { get; set; }

        /// <summary>
        /// Optional current status of the game (e.g., Released, In Development).
        /// </summary>
        public string? GameStatus { get; set; }

        // -------------------------
        // Aggregate Metadata
        // -------------------------
        /// <summary>
        /// Timestamp when the game was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Optional timestamp when the game was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Indicates whether the game is currently active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
