using Microsoft.Extensions.Logging;
//TODO: This file can be deleted
namespace dotFitness.Api.Infrastructure;

/// <summary>
/// Handles MongoDB index configuration for the application
/// </summary>
public static class MongoDbIndexConfigurator
{
    /// <summary>
    /// Configures all MongoDB indexes for the application
    /// </summary>
    public static async Task ConfigureIndexesAsync(IServiceProvider services)
    {
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("MongoDbIndexConfigurator");
        logger.LogInformation("Relational DB in use; Mongo index configuration is skipped.");
    }
} 