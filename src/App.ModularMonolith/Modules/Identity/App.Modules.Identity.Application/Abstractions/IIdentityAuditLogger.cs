namespace App.Modules.Identity.Application.Abstractions;

public interface IIdentityAuditLogger
{
    Task LogAsync(Guid? userId, string action, string? details, CancellationToken ct);
}


