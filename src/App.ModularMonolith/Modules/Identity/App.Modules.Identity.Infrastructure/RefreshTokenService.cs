using System.Security.Cryptography;
using System.Text;
using App.Modules.Identity.Application.Abstractions;
using App.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Identity.Infrastructure;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IdentityDbContext _dbContext;
    private readonly IIdentityService _identityService;

    public RefreshTokenService(IdentityDbContext dbContext, IIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public async Task<(string accessToken, string refreshToken)> IssueAsync(Guid userId, string email, string displayName, IEnumerable<System.Security.Claims.Claim> extraClaims, CancellationToken ct)
    {
        var refreshToken = GenerateSecureToken();
        var refreshTokenHash = Hash(refreshToken);
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = refreshTokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
        };
        _dbContext.Set<RefreshToken>().Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        var accessToken = await _identityService.GenerateJwtTokenAsync(userId.ToString(), email, displayName, extraClaims, ct);
        return (accessToken, refreshToken);
    }

    public async Task<(string accessToken, string refreshToken)?> RotateAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var entity = await _dbContext.Set<RefreshToken>().FirstOrDefaultAsync(x => x.TokenHash == hash && x.RevokedAtUtc == null, ct);
        if (entity == null || entity.ExpiresAtUtc < DateTime.UtcNow) return null;

        entity.RevokedAtUtc = DateTime.UtcNow;

        var newToken = GenerateSecureToken();
        var newHash = Hash(newToken);
        var replacement = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = entity.UserId,
            TokenHash = newHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
        };
        entity.ReplacedByTokenHash = newHash;
        _dbContext.Set<RefreshToken>().Add(replacement);
        await _dbContext.SaveChangesAsync(ct);

        // Build new access token: need user basic info; for demo keep minimal
        var user = await _dbContext.Users.FirstAsync(u => u.Id == entity.UserId, ct);
        var accessToken = await _identityService.GenerateJwtTokenAsync(user.Id.ToString(), user.Email!, user.UserName!, Array.Empty<System.Security.Claims.Claim>(), ct);
        return (accessToken, newToken);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var entity = await _dbContext.Set<RefreshToken>().FirstOrDefaultAsync(x => x.TokenHash == hash && x.RevokedAtUtc == null, ct);
        if (entity == null) return;
        entity.RevokedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string Hash(string input)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(input)));
    }
}


