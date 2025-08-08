using App.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Users.Infrastructure.Persistence;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<UserMetric> UserMetrics => Set<UserMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserMetricConfiguration());
    }
}
