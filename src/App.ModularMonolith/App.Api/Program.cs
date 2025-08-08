using Serilog;
using App.Api.Infrastructure;
using App.Api.Infrastructure.Extensions;
using App.Api.Infrastructure.Settings;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog as the logging provider
builder.Host.UseSerilog();

// Configure application settings
builder.Services.Configure<GoogleOAuthSettings>(builder.Configuration.GetSection("GoogleOAuth"));

// Add core API services
builder.Services.AddCoreApiServices();

// Add Swagger with OAuth2 support
builder.Services.AddSwaggerWithOAuth();

// Add CORS policy
builder.Services.AddCorsPolicy();

// Database services are configured per-module (Users module wires EF Core)

// Set up module registry logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(Log.Logger));
var microsoftLogger = loggerFactory.CreateLogger("ModuleRegistry");

// Add module services
builder.Services.AddModuleServices(builder.Configuration, microsoftLogger);

var app = builder.Build();

// Ensure EF Core databases are created (template-friendly; swap to migrations for production)
RelationalDbInitializer.EnsureDatabasesCreated(app.Services, microsoftLogger);

// Configure the application pipeline
app.ConfigureSwaggerUi()
   .ConfigureCoreMiddleware()
   .ConfigureHealthChecks()
   .ConfigureEndpoints();

Log.Information("App API starting up...");

app.Run();
