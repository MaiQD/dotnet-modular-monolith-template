using App.Modules.Identity.Application.Abstractions;
using App.Modules.Identity.Domain.Entities;

namespace App.Modules.Identity.Infrastructure;

public class IdentityAuditLogger : IIdentityAuditLogger
{
    private readonly IdentityDbContext _dbContext;
    public IdentityAuditLogger(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(Guid? userId, string action, string? details, CancellationToken ct)
    {
        _dbContext.Set<IdentityAuditLog>().Add(new IdentityAuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            Details = details,
            OccurredAtUtc = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync(ct);
    }
}


