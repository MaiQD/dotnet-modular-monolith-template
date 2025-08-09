using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace App.Modules.Identity.Infrastructure;

public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        // Prefer env var; fallback to default local connection for design-time
        var conn = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__POSTGRESQL")
                   ?? "Host=localhost;Port=5432;Database=app_modularmonolith;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(conn, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "identity"));
        return new IdentityDbContext(optionsBuilder.Options);
    }
}


