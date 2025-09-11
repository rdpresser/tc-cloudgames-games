﻿namespace TC.CloudGames.Games.Application.UseCases.CreateGame
{
    public sealed record CreateGameCommand(
        string Name,
        DateOnly ReleaseDate,
        string AgeRating,
        string? Description,
        DeveloperInfo DeveloperInfo,
        decimal DiskSize,
        decimal Price,
        Playtime? Playtime,
        GameDetails GameDetails,
        SystemRequirements SystemRequirements,
        decimal? Rating,
        string? OfficialLink,
        string? GameStatus)
        : IBaseCommand<CreateGameResponse>;

    public sealed record DeveloperInfo(string Developer, string? Publisher);

    public sealed record Price(decimal Amount);

    public sealed record Playtime(int? Hours, int? PlayerCount);

    public sealed record SystemRequirements(string Minimum, string? Recommended);

    public sealed record GameDetails(
        string? Genre,
        string[] Platforms,
        string? Tags,
        string GameMode,
        string DistributionFormat,
        string? AvailableLanguages,
        bool SupportsDlcs
    );
}
