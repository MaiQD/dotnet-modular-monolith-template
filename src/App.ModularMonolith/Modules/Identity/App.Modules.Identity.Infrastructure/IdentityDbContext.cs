using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App.Modules.Identity.Domain.Entities;

namespace App.Modules.Identity.Infrastructure;

public class IdentityDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("identity");

        builder.Entity<RefreshToken>(b =>
        {
            b.ToTable("RefreshTokens");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.TokenHash).IsUnique();
            b.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });
        });

        builder.Entity<IdentityAuditLog>(b =>
        {
            b.ToTable("IdentityAuditLogs");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.UserId, x.OccurredAtUtc });
        });
    }
}


