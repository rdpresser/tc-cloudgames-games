using Shouldly;
using TC.CloudGames.Games.Domain.ValueObjects;

namespace TC.CloudGames.Games.Unit.Tests.Domain.ValueObjects
{
    public class GameDetailsTests
    {
        private readonly List<string> _validPlatforms = new() { "PC", "PlayStation 5" };

        [Fact]
        public void Create_ShouldReturnSuccess_WhenAllValuesAreValid()
        {
            // Act
            var result = GameDetails.Create(
                "Action",
                _validPlatforms,
                "Tag1, Tag2",
                "Singleplayer",
                "Digital",
                "English, Portuguese",
                true);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Genre.ShouldBe("Action");
            result.Value.Platforms.ShouldContain("PC");
            result.Value.GameMode.ShouldBe("Singleplayer");
            result.Value.DistributionFormat.ShouldBe("Digital");
            result.Value.SupportsDlcs.ShouldBeTrue();
        }

        [Fact]
        public void Create_ShouldReturnError_WhenGenreExceedsMaxLength()
        {
            var longGenre = new string('G', 51);

            var result = GameDetails.Create(
                longGenre,
                _validPlatforms,
                "Tags",
                "Singleplayer",
                "Digital",
                null,
                true);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(GameDetails.GenreMaximumLength);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenNoPlatformsProvided()
        {
            var result = GameDetails.Create(
                "RPG",
                new List<string>(),
                "Tags",
                "Singleplayer",
                "Digital",
                null,
                true);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(GameDetails.PlatformRequired);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenInvalidPlatformProvided()
        {
            var result = GameDetails.Create(
                "RPG",
                new List<string> { "InvalidConsole" },
                null,
                "Singleplayer",
                "Digital",
                null,
                true);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(GameDetails.PlatformInvalid);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenTagsExceedMaxLength()
        {
            var longTags = new string('T', 201);

            var result = GameDetails.Create(
                "RPG",
                _validPlatforms,
                longTags,
                "Singleplayer",
                "Digital",
                null,
                true);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(GameDetails.TagsMaximumLength);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenGameModeIsMissingOrInvalid()
        {
            var result1 = GameDetails.Create(
                "RPG",
                _validPlatforms,
                null,
                "",
                "Digital",
                null,
                true);

            result1.IsSuccess.ShouldBeFalse();
            result1.ValidationErrors.ShouldContain(GameDetails.GameModeRequired);

            var result2 = GameDetails.Create(
                "RPG",
                _validPlatforms,
                null,
                "InvalidMode",
                "Digital",
                null,
                true);

            result2.IsSuccess.ShouldBeFalse();
            result2.ValidationErrors.ShouldContain(GameDetails.GameModeInvalid);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenDistributionFormatIsMissingOrInvalid()
        {
            var result1 = GameDetails.Create(
                "RPG",
                _validPlatforms,
                null,
                "Singleplayer",
                "",
                null,
                true);

            result1.IsSuccess.ShouldBeFalse();
            result1.ValidationErrors.ShouldContain(GameDetails.DistributionFormatRequired);

            var result2 = GameDetails.Create(
                "RPG",
                _validPlatforms,
                null,
                "Singleplayer",
                "Cartridge",
                null,
                true);

            result2.IsSuccess.ShouldBeFalse();
            result2.ValidationErrors.ShouldContain(GameDetails.DistributionFormatInvalid);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenAvailableLanguagesExceedMaxLength()
        {
            var longLanguages = new string('L', 101);

            var result = GameDetails.Create(
                "RPG",
                _validPlatforms,
                null,
                "Singleplayer",
                "Digital",
                longLanguages,
                true);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(GameDetails.AvailableLanguagesMaximumLength);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenSupportsDlcsIsMissing()
        {
            var result = GameDetails.TryValidateValue(
                "RPG",
                _validPlatforms,
                null,
                "Singleplayer",
                "Digital",
                null,
                null,
                out var errors);

            result.ShouldBeFalse();
            errors.ShouldContain(GameDetails.SupportsDlcsRequired);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenInstanceIsNull()
        {
            var result = GameDetails.Validate(null);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(GameDetails.PlatformRequired);
            result.ValidationErrors.ShouldContain(GameDetails.GameModeRequired);
            result.ValidationErrors.ShouldContain(GameDetails.DistributionFormatRequired);
            result.ValidationErrors.ShouldContain(GameDetails.SupportsDlcsRequired);
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenInstanceIsValid()
        {
            var gameDetails = GameDetails.Create(
                "Adventure",
                _validPlatforms,
                "Tag",
                "Multiplayer",
                "Physical",
                "English",
                false).Value;

            var isValid = GameDetails.IsValid(gameDetails);

            isValid.ShouldBeTrue();
        }

        [Fact]
        public void FromDb_ShouldReturnSuccess_WhenValuesAreValid()
        {
            var result = GameDetails.FromDb(
                "Adventure",
                _validPlatforms,
                "Tag",
                "PvE",
                "Digital",
                "English",
                true);

            result.IsSuccess.ShouldBeTrue();
            result.Value.GameMode.ShouldBe("PvE");
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            var gameDetails = GameDetails.Create(
                "Action",
                _validPlatforms,
                null,
                "Singleplayer",
                "Digital",
                null,
                true).Value;

            var str = gameDetails.ToString();

            str.ShouldBe("PC, PlayStation 5 - Singleplayer - Digital");
        }
    }
}