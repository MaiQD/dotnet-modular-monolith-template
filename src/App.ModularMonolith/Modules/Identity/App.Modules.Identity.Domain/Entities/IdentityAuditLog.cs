namespace App.Modules.Identity.Domain.Entities;

public class IdentityAuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // e.g., LoginSuccess, LoginFailed, PasswordResetRequested, EmailConfirmed
    public string? Details { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}


