﻿namespace TC.CloudGames.Games.Api.Telemetry
{
    /// <summary>
    /// Constants for telemetry across the application
    /// </summary>
    internal static class TelemetryConstants
    {
        // Versions
        public const string Version = "1.0.0";

        // Service Identity - Centralized for consistency (matches Docker Compose)
        public const string ServiceName = "tccloudgames-games";
        public const string ServiceNamespace = "tccloudgames";

        // Meter Names for OpenTelemetry Metrics
        public const string GamesMeterName = "TC.CloudGames.Games.Api.Metrics";

        // Activity Source Names for OpenTelemetry Tracing
        public const string GameActivitySource = "TC.CloudGames.Games.Api";
        public const string DatabaseActivitySource = "TC.CloudGames.Games.Api.Database";
        public const string CacheActivitySource = "TC.CloudGames.Games.Api.Cache";

        // Header Names (standardized)
        public const string CorrelationIdHeader = "x-correlation-id";

        // Tag Names (using underscores for consistency with Loki labels)
        public const string ServiceComponent = "service.component";
        public const string GameType = "game_type";
        public const string UserId = "user_id";
        public const string GameId = "game_id";
        public const string GameDifficulty = "game_difficulty";
        public const string GameStatus = "game_status";
        public const string UserAction = "user_action";
        public const string SessionId = "session_id";
        public const string ErrorType = "error_type";

        // Default Values
        public const string DefaultDifficulty = "normal";
        public const string DefaultGameType = "unknown";
        public const string AnonymousUser = "anonymous";

        // Service Components
        public const string GameComponent = "game";
        public const string DatabaseComponent = "database";
        public const string CacheComponent = "cache";

        /// <summary>
        /// Logs telemetry configuration details using Microsoft.Extensions.Logging.ILogger
        /// </summary>
        public static void LogTelemetryConfiguration(Microsoft.Extensions.Logging.ILogger logger)
        {
            logger.LogInformation("=== TELEMETRY DEBUG INFO ===");
            logger.LogInformation("Service Name: {ServiceName}", ServiceName);
            logger.LogInformation("Service Namespace: {ServiceNamespace}", ServiceNamespace);
            logger.LogInformation("Telemetry Version: {Version}", Version);
            logger.LogInformation("Correlation Header: {CorrelationIdHeader}", CorrelationIdHeader);
            logger.LogInformation("Game Meter: {GameMeterName}", GamesMeterName);
            logger.LogInformation("Game Activity Source: {GameActivitySource}", GameActivitySource);
            logger.LogInformation("Database Activity Source: {DatabaseActivitySource}", DatabaseActivitySource);
            logger.LogInformation("Cache Activity Source: {CacheActivitySource}", CacheActivitySource);
            logger.LogInformation("Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "NOT SET");
            logger.LogInformation("Machine Name: {MachineName}", Environment.MachineName);
            logger.LogInformation("Container Name: {ContainerName}", Environment.GetEnvironmentVariable("HOSTNAME") ?? "NOT SET");

            // OTLP Configuration Debug (no sensitive values)
            var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
            var otlpHeaders = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS");
            var otlpProtocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL");
            var grafanaApiToken = Environment.GetEnvironmentVariable("GRAFANA_LOGS_API_TOKEN");
            var grafanaPrometheusToken = Environment.GetEnvironmentVariable("GRAFANA_OTEL_PROMETHEUS_API_TOKEN");

            logger.LogInformation("OTLP Endpoint: {OtlpEndpoint}", string.IsNullOrEmpty(otlpEndpoint) ? "NOT SET" : otlpEndpoint);
            logger.LogInformation("OTLP Headers: {OtlpHeaders}", string.IsNullOrEmpty(otlpHeaders) ? "NOT SET" : "***CONFIGURED***");
            logger.LogInformation("OTLP Protocol: {OtlpProtocol}", string.IsNullOrEmpty(otlpProtocol) ? "NOT SET" : otlpProtocol);
            logger.LogInformation("Grafana API Token: {GrafanaApiToken}", string.IsNullOrEmpty(grafanaApiToken) ? "NOT SET" : "***CONFIGURED***");
            logger.LogInformation("Grafana Prometheus Token: {GrafanaPrometheusToken}", string.IsNullOrEmpty(grafanaPrometheusToken) ? "NOT SET" : "***CONFIGURED***");
            logger.LogInformation("============================");
        }
    }
}
