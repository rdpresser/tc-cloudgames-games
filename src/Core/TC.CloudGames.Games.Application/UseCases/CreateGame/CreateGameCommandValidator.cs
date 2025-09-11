﻿namespace TC.CloudGames.Games.Application.UseCases.CreateGame
{
    public sealed class CreateGameCommandValidator : Validator<CreateGameCommand>
    {
        public CreateGameCommandValidator()
        {
            ValidateName();
            ValidateReleaseDate();
            ValidateAgeRating();
            ValidateDescription();
            ValidateDeveloperInfo();
            ValidateDiskSize();
            ValidatePrice();
            ValidatePlaytime();
            ValidateGameDetails();
            ValidateSystemRequirements();
            ValidateRating();
            ValidateGameStatus();
            ValidateOfficialLink();
        }

        private void ValidateName()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("Game name is required.")
                    .WithErrorCode($"{nameof(GameAggregate.Name)}.Required")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Name)
                        .MaximumLength(200)
                            .WithMessage("Name cannot exceed 200 characters.")
                            .WithErrorCode($"{nameof(GameAggregate.Name)}.MaximumLength");
                });
        }

        private void ValidateReleaseDate()
        {
            RuleFor(x => x.ReleaseDate)
                .NotEmpty()
                    .WithMessage("Release date is required.")
                    .WithErrorCode($"{nameof(GameAggregate.ReleaseDate)}.Required")
                .Must(date => date > DateOnly.MinValue)
                    .WithMessage("Release date must be a valid date.")
                    .WithErrorCode($"{nameof(GameAggregate.ReleaseDate)}.ValidDate");
        }

        private void ValidateAgeRating()
        {
            RuleFor(x => x.AgeRating)
                .NotEmpty()
                    .WithMessage("Age rating object is required.")
                    .WithErrorCode($"{nameof(GameAggregate.AgeRating)}.Required")
                .DependentRules(() =>
                {
                    RuleFor(x => x.AgeRating)
                        .NotEmpty()
                            .WithMessage("Age rating value is required.")
                            .WithErrorCode($"{nameof(GameAggregate.AgeRating)}.Required")
                        .MaximumLength(10)
                            .WithMessage("Age rating value cannot exceed 10 characters.")
                            .WithErrorCode($"{nameof(GameAggregate.AgeRating)}.MaximumLength");
                })
                .Must(rating => AgeRating.ValidRatings.Contains(rating))
                    .WithMessage($"Invalid age rating value specified. Valid age rating are: {AgeRating.ValidRatings.JoinWithQuotes()}.")
                    .WithErrorCode($"{nameof(GameAggregate.AgeRating)}.ValidRating");
        }

        private void ValidateDescription()
        {
            RuleFor(game => game.Description)
                .MaximumLength(2000)
                    .WithMessage("Description cannot exceed 2000 characters.")
                    .WithErrorCode($"{nameof(GameAggregate.Description)}.MaximumLength");
        }

        private void ValidateDeveloperInfo()
        {
            RuleFor(x => x.DeveloperInfo)
                .NotNull()
                    .WithMessage("Developer information object is required.")
                    .WithErrorCode($"{nameof(GameAggregate.DeveloperInfo)}.Required")
                .DependentRules(() =>
                {
                    RuleFor(x => x.DeveloperInfo.Developer)
                        .NotEmpty()
                            .WithMessage("Developer name is required.")
                            .WithErrorCode($"{nameof(DeveloperInfo.Developer)}.Required")
                        .MaximumLength(100)
                            .WithMessage("Developer name must not exceed 100 characters.")
                            .WithErrorCode($"{nameof(DeveloperInfo.Developer)}.MaximumLength");

                    When(x => x.DeveloperInfo.Publisher != null, () =>
                    {
                        RuleFor(x => x.DeveloperInfo.Publisher)
                            .MaximumLength(200)
                                .WithMessage("Publisher name must not exceed 200 characters.")
                                .WithErrorCode($"{nameof(DeveloperInfo.Publisher)}.MaximumLength");
                    });
                });
        }

        private void ValidateDiskSize()
        {
            RuleFor(x => x.DiskSize)
                .NotNull()
                    .WithMessage("Disk size object is required.")
                    .WithErrorCode($"{nameof(GameAggregate.DiskSize)}.Required")
                .DependentRules(() =>
                {
                    RuleFor(x => x.DiskSize)
                        .GreaterThan(0)
                            .WithMessage("Disk size value must be greater than 0.")
                            .WithErrorCode($"{nameof(GameAggregate.DiskSize)}.GreaterThanZero");
                });
        }

        private void ValidatePrice()
        {
            RuleFor(x => x.Price)
                .NotNull()
                    .WithMessage("Price object is required.")
                    .WithErrorCode($"{nameof(GameAggregate.Price)}.Required")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Price)
                        .GreaterThanOrEqualTo(0)
                            .WithMessage("Price amount must be greater than or equal to 0.")
                            .WithErrorCode($"{nameof(GameAggregate.Price)}.GreaterThanOrEqualToZero");
                });
        }

        private void ValidatePlaytime()
        {
            RuleFor(x => x.Playtime)
                .Must(playtime => playtime == null || playtime.Hours != null || playtime.PlayerCount != null)
                    .WithMessage("At least one of Playtime Hours or Player Count must be provided for Playtime object.")
                    .WithErrorCode($"{nameof(GameAggregate.Playtime)}.Required")
                .DependentRules(() =>
                {
                    When(x => x.Playtime?.Hours != null, () =>
                    {
                        RuleFor(x => x.Playtime!.Hours)
                            .GreaterThanOrEqualTo(0)
                                .WithMessage("Playtime hours must be greater than or equal to 0.")
                                .WithErrorCode($"{nameof(Playtime.Hours)}.GreaterThanOrEqualToZero");
                    });

                    When(x => x.Playtime?.PlayerCount != null, () =>
                    {
                        RuleFor(x => x.Playtime!.PlayerCount)
                            .GreaterThanOrEqualTo(1)
                                .WithMessage("Player count must be greater than or equal to 1.")
                                .WithErrorCode($"{nameof(Playtime.PlayerCount)}.GreaterThanOrEqualToZero");
                    });
                });
        }

        private void ValidateGameDetails()
        {
            RuleFor(x => x.GameDetails)
                .NotNull()
                    .WithMessage("Game details object are required.")
                    .WithErrorCode($"{nameof(GameAggregate.GameDetails)}.Required")
                .DependentRules(() =>
                {
                    When(x => x.GameDetails.Genre != null, () =>
                    {
                        RuleFor(x => x.GameDetails.Genre)
                            .MaximumLength(50)
                                .WithMessage("Genre must not exceed 50 characters.")
                                .WithErrorCode($"{nameof(GameDetails.Genre)}.MaximumLength");
                    });

                    RuleFor(x => x.GameDetails.Platforms)
                        .NotEmpty()
                            .WithMessage("Platform is required.")
                            .WithErrorCode($"{nameof(GameDetails.Platforms)}.Required")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.GameDetails.Platforms)
                                .Must(platform => Domain.ValueObjects.GameDetails.ValidPlatforms.All(x => platform.Contains(x)))
                                    .WithMessage($"Invalid platform specified. Valid platforms are: {Domain.ValueObjects.GameDetails.ValidPlatforms.JoinWithQuotes()}.")
                                    .WithErrorCode($"{nameof(GameDetails.Platforms)}.ValidPlatform");
                        });

                    When(x => x.GameDetails.Tags != null, () =>
                    {
                        RuleFor(x => x.GameDetails.Tags)
                            .MaximumLength(200)
                                .WithMessage("Tags must not exceed 200 characters.")
                                .WithErrorCode($"{nameof(GameDetails.Tags)}.MaximumLength");
                    });

                    RuleFor(x => x.GameDetails.GameMode)
                        .NotEmpty()
                            .WithMessage("Game mode is required.")
                            .WithErrorCode($"{nameof(GameDetails.GameMode)}.Required")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.GameDetails.GameMode)
                                .Must(mode => Domain.ValueObjects.GameDetails.ValidGameModes.Contains(mode))
                                    .WithMessage($"Invalid game mode specified. Valid game modes are: {Domain.ValueObjects.GameDetails.ValidGameModes.JoinWithQuotes()}.")
                                    .WithErrorCode($"{nameof(GameDetails.GameMode)}.ValidGameMode");
                        });

                    RuleFor(x => x.GameDetails.DistributionFormat)
                        .NotEmpty()
                            .WithMessage("Distribution format is required.")
                            .WithErrorCode($"{nameof(GameDetails.DistributionFormat)}.Required")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.GameDetails.DistributionFormat)
                                .Must(format => Domain.ValueObjects.GameDetails.ValidDistributionFormats.Contains(format))
                                    .WithMessage($"Invalid distribution format specified. Valid formats are: {Domain.ValueObjects.GameDetails.ValidDistributionFormats.JoinWithQuotes()}.")
                                    .WithErrorCode($"{nameof(GameDetails.DistributionFormat)}.ValidDistributionFormat");
                        });

                    When(x => x.GameDetails.AvailableLanguages != null, () =>
                    {
                        RuleFor(x => x.GameDetails.AvailableLanguages)
                            .MaximumLength(100)
                                .WithMessage("Available languages must not exceed 100 characters.")
                                .WithErrorCode($"{nameof(GameDetails.AvailableLanguages)}.MaximumLength");
                    });

                    RuleFor(x => x.GameDetails.SupportsDlcs)
                        .NotNull()
                            .WithMessage("Supports DLCs field is required.")
                            .WithErrorCode($"{nameof(GameDetails.SupportsDlcs)}.Required");
                });
        }

        private void ValidateSystemRequirements()
        {
            RuleFor(x => x.SystemRequirements)
                .NotNull()
                    .WithMessage("System requirements object are required.")
                    .WithErrorCode($"{nameof(GameAggregate.SystemRequirements)}.Required")
                .DependentRules(() =>
                {
                    RuleFor(x => x.SystemRequirements.Minimum)
                        .NotEmpty()
                            .WithMessage("Minimum system requirements are required.")
                            .WithErrorCode($"{nameof(SystemRequirements.Minimum)}.Required")
                        .MaximumLength(1000)
                            .WithMessage("Minimum system requirements must not exceed 1000 characters.")
                            .WithErrorCode($"{nameof(SystemRequirements.Minimum)}.MaximumLength");
                });
        }

        private void ValidateRating()
        {
            When(x => x.Rating != null, () =>
            {
                RuleFor(x => x.Rating!)
                    .GreaterThanOrEqualTo(0)
                        .WithMessage("Rating value must be greater than or equal to 0.")
                        .WithErrorCode($"{nameof(GameAggregate.Rating)}.GreaterThanOrEqualToZero")
                    .LessThanOrEqualTo(10)
                        .WithMessage("Rating value must be less than or equal to 10.")
                        .WithErrorCode($"{nameof(GameAggregate.Rating)}.LessThanOrEqualToTen");
            });
        }

        private void ValidateGameStatus()
        {
            When(x => x.GameStatus != null, () =>
            {
                RuleFor(x => x.GameStatus)
                .Must(status => GameAggregate.ValidGameStatus.Contains(status!))
                    .WithMessage($"Invalid game status specified. Valid status are: {GameAggregate.ValidGameStatus.JoinWithQuotes()}.")
                    .WithErrorCode($"{nameof(GameAggregate.GameStatus)}.ValidGameStatus")
                .MaximumLength(200)
                    .WithMessage("Game status must not exceed 200 characters.")
                    .WithErrorCode($"{nameof(GameAggregate.GameStatus)}.MaximumLength");
            });
        }

        private void ValidateOfficialLink()
        {
            When(x => !string.IsNullOrWhiteSpace(x.OfficialLink), () =>
            {
                RuleFor(game => game.OfficialLink)
                    .MaximumLength(200)
                        .WithMessage("Official link must not exceed 200 characters.")
                        .WithErrorCode($"{nameof(GameAggregate.OfficialLink)}.MaximumLength")
                    .Must(link => Uri.IsWellFormedUriString(link, UriKind.Absolute))
                        .WithMessage("Official link must be a valid URL.")
                        .WithErrorCode($"{nameof(GameAggregate.OfficialLink)}.ValidUrl");
            });
        }
    }
}
