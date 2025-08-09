using System.Security.Claims;

namespace App.Modules.Identity.Application.Abstractions;

public interface IIdentityService
{
    Task<string> GenerateJwtTokenAsync(string userId, string email, string displayName, IEnumerable<Claim> extraClaims, CancellationToken cancellationToken);
}


