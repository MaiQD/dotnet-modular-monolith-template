using dotFitness.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dotFitness.Modules.Users.Infrastructure.Persistence;

public class UserMetricConfiguration : IEntityTypeConfiguration<UserMetric>
{
    public void Configure(EntityTypeBuilder<UserMetric> builder)
    {
        builder.ToTable("user_metrics");
        builder.HasKey(um => um.Id);
        builder.Property(um => um.Id).HasMaxLength(64);
        builder.Property(um => um.UserId).IsRequired().HasMaxLength(64);
        builder.Property(um => um.Date).IsRequired();
        builder.Property(um => um.Notes).HasMaxLength(1000);
        builder.Property(um => um.CreatedAt).IsRequired();
        builder.Property(um => um.UpdatedAt).IsRequired();

        builder.HasIndex(um => new { um.UserId, um.Date }).IsUnique();
        builder.HasIndex(um => um.UserId);
        builder.HasIndex(um => um.Date);
    }
}
