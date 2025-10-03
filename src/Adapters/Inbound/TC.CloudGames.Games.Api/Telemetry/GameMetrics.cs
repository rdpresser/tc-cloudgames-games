﻿using System.Diagnostics.Metrics;

namespace TC.CloudGames.Games.Api.Telemetry
{
    [ExcludeFromCodeCoverage]
    public class GameMetrics
    {
        // Counters for game actions
        private readonly Counter<long> _gamesCreated;
        private readonly Counter<long> _gamesCompleted;
        private readonly Counter<long> _gameErrors;
        private readonly Counter<long> _userActions;

        // Histograms for performance
        private readonly Histogram<double> _gameLoadTime;
        private readonly Histogram<double> _gameDuration;

        // Up-down counters for current state
        private readonly UpDownCounter<long> _activeGames;

        public GameMetrics()
        {
            var meter = new Meter(TelemetryConstants.GamesMeterName, TelemetryConstants.Version);

            _gamesCreated = meter.CreateCounter<long>(
                "games_created_total",
                description: "Total number of games created");

            _gamesCompleted = meter.CreateCounter<long>(
                "games_completed_total",
                description: "Total number of games completed");

            _gameErrors = meter.CreateCounter<long>(
                "game_errors_total",
                description: "Total number of game errors");

            _gameLoadTime = meter.CreateHistogram<double>(
                "game_load_time_seconds",
                unit: "s",
                description: "Time taken to load a game");

            _gameDuration = meter.CreateHistogram<double>(
                "game_duration_seconds",
                unit: "s",
                description: "Total duration of completed games");

            _activeGames = meter.CreateUpDownCounter<long>(
                "active_games",
                description: "Number of currently active games");

            _userActions = meter.CreateCounter<long>(
                "user_actions_total",
                description: "Total number of user actions");
        }

        /// <summary>
        /// Records general user actions
        /// </summary>
        public void RecordUserAction(string action, string userId, string details = "") =>
            _userActions.Add(1,
                new KeyValuePair<string, object?>(TelemetryConstants.UserAction, action),
                new KeyValuePair<string, object?>(TelemetryConstants.UserId, userId),
                new KeyValuePair<string, object?>("action_details", details));

        /// <summary>
        /// Records when a new game is created
        /// </summary>
        public void RecordGameCreated(string gameType, string difficulty = TelemetryConstants.DefaultDifficulty, string userId = TelemetryConstants.AnonymousUser) =>
            _gamesCreated.Add(1,
                new KeyValuePair<string, object?>(TelemetryConstants.GameType, gameType),
                new KeyValuePair<string, object?>(TelemetryConstants.GameDifficulty, difficulty),
                new KeyValuePair<string, object?>(TelemetryConstants.UserId, userId));

        /// <summary>
        /// Records when a game is completed
        /// </summary>
        public void RecordGameCompleted(string gameId, string gameType, double durationSeconds, string difficulty = TelemetryConstants.DefaultDifficulty, string userId = TelemetryConstants.AnonymousUser)
        {
            _gamesCompleted.Add(1,
                new KeyValuePair<string, object?>(TelemetryConstants.GameId, gameId),
                new KeyValuePair<string, object?>(TelemetryConstants.GameType, gameType),
                new KeyValuePair<string, object?>(TelemetryConstants.GameDifficulty, difficulty),
                new KeyValuePair<string, object?>(TelemetryConstants.UserId, userId));

            _gameDuration.Record(durationSeconds,
                new KeyValuePair<string, object?>(TelemetryConstants.GameType, gameType),
                new KeyValuePair<string, object?>(TelemetryConstants.GameDifficulty, difficulty));
        }

        /// <summary>
        /// Records game errors
        /// </summary>
        public void RecordGameError(string gameId, string gameType, string errorType, string userId = TelemetryConstants.AnonymousUser) =>
            _gameErrors.Add(1,
                new KeyValuePair<string, object?>(TelemetryConstants.GameId, gameId),
                new KeyValuePair<string, object?>(TelemetryConstants.GameType, gameType),
                new KeyValuePair<string, object?>(TelemetryConstants.ErrorType, errorType),
                new KeyValuePair<string, object?>(TelemetryConstants.UserId, userId));

        /// <summary>
        /// Records game load time
        /// </summary>
        public void RecordGameLoadTime(double loadTimeSeconds, string gameType, string difficulty = TelemetryConstants.DefaultDifficulty) =>
            _gameLoadTime.Record(loadTimeSeconds,
                new KeyValuePair<string, object?>(TelemetryConstants.GameType, gameType),
                new KeyValuePair<string, object?>(TelemetryConstants.GameDifficulty, difficulty));

        /// <summary>
        /// Increments active games counter
        /// </summary>
        public void GameStarted(string gameType, string difficulty = TelemetryConstants.DefaultDifficulty) =>
            _activeGames.Add(1,
                new KeyValuePair<string, object?>(TelemetryConstants.GameType, gameType),
                new KeyValuePair<string, object?>(TelemetryConstants.GameDifficulty, difficulty));

        /// <summary>
        /// Decrements active games counter
        /// </summary>
        public void GameEnded(string gameType, string difficulty = TelemetryConstants.DefaultDifficulty) =>
            _activeGames.Add(-1,
                new KeyValuePair<string, object?>(TelemetryConstants.GameType, gameType),
                new KeyValuePair<string, object?>(TelemetryConstants.GameDifficulty, difficulty));
    }
}
