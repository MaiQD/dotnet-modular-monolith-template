using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace dotFitness.Api.Infrastructure;

/// <summary>
/// Initializes relational databases by discovering DbContext types via reflection
/// and invoking EnsureCreated on each registered context.
/// </summary>
public static class RelationalDbInitializer
{
    public static void EnsureDatabasesCreated(IServiceProvider serviceProvider, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        var dbContextTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .Where(t => !t.IsAbstract && typeof(DbContext).IsAssignableFrom(t))
            .Distinct()
            .ToList();

        foreach (var dbContextType in dbContextTypes)
        {
            try
            {
                var ctx = sp.GetService(dbContextType) as DbContext;
                if (ctx == null) continue; // Not registered in DI

                logger.LogInformation("Ensuring database created for DbContext: {DbContext}", dbContextType.FullName);
                ctx.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to ensure database for DbContext: {DbContext}", dbContextType.FullName);
            }
        }
    }
}
