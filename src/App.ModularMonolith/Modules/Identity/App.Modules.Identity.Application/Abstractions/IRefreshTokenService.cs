namespace App.Modules.Identity.Application.Abstractions;

public interface IRefreshTokenService
{
    Task<(string accessToken, string refreshToken)> IssueAsync(Guid userId, string email, string displayName, IEnumerable<System.Security.Claims.Claim> extraClaims, CancellationToken ct);
    Task<(string accessToken, string refreshToken)?> RotateAsync(string refreshToken, CancellationToken ct);
    Task RevokeAsync(string refreshToken, CancellationToken ct);
}


