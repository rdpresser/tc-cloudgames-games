using TC.CloudGames.Games.Search;

var builder = WebApplication.CreateBuilder(args);

// Configure environment variables (will skip if running under .NET Aspire)
builder.ConfigureEnvironmentVariables();

// Configure Serilog as logging provider
builder.Host.UseCustomSerilog(builder.Configuration);

//***************** ADICIONAR **************************************************/
//builder.AddCustomLoggingTelemetry()
//********************************************************************************/

// Register application, infrastructure and API services
builder.Services.AddGameServices(builder);
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddElasticSearch(builder.Configuration);

var app = builder.Build();

// ElasticSearch index initialization
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<GamesIndexInitializer>();
    await initializer.InitializeAsync();
}

if (!builder.Environment.IsEnvironment("Testing"))
{
    await app.CreateMessageDatabase().ConfigureAwait(false);
}

// Use metrics authentication middleware extension
app.UseMetricsAuthentication();

app.UseAuthentication()
  .UseAuthorization()
  .UseCustomFastEndpoints()
  .UseCustomMiddlewares();

// Run the application
await app.RunAsync().ConfigureAwait(false);