var builder = WebApplication.CreateBuilder(args);

// Configure environment variables (will skip if running under .NET Aspire)
builder.ConfigureEnvironmentVariables();

// Configure Serilog as logging provider
builder.Host.UseCustomSerilog(builder.Configuration);

// Register application, infrastructure and API services
builder.Services.AddGameServices(builder);
builder.Services.AddApplication();

// Create a temporary logger factory for infrastructure setup logging
using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
});
var setupLogger = loggerFactory.CreateLogger("Infrastructure.Setup");

// Register infrastructure services including Elasticsearch
builder.Services
    .AddInfrastructure()
    .AddElasticSearch(builder.Configuration, setupLogger);

var app = builder.Build();

if (!builder.Environment.IsEnvironment("Testing"))
{
    await app.CreateMessageDatabase().ConfigureAwait(false);
}

// Get logger instance for Program and log telemetry configuration
var logger = app.Services.GetRequiredService<ILogger<TC.CloudGames.Games.Api.Program>>();
TelemetryConstants.LogTelemetryConfiguration(logger, app.Configuration);

// Log APM/exporter configuration (Azure Monitor, OTLP, etc.)
// This info was populated during service configuration in ServiceCollectionExtensions
var exporterInfo = app.Services.GetService<TC.CloudGames.Games.Api.Extensions.TelemetryExporterInfo>();
TelemetryConstants.LogApmExporterConfiguration(logger, exporterInfo);

// Use metrics authentication middleware extension
app.UseIngressPathBase(app.Configuration);
app.UseMetricsAuthentication();

app.UseAuthentication()
  .UseAuthorization()
  .UseCustomFastEndpoints(app.Configuration)
  .UseCustomMiddlewares();

// Run the application
await app.RunAsync().ConfigureAwait(false);