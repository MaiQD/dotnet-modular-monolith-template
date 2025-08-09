using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using App.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace App.Modules.Identity.Infrastructure;

public class JwtIdentityService : IIdentityService
{
    private readonly IConfiguration _configuration;

    public JwtIdentityService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string> GenerateJwtTokenAsync(string userId, string email, string displayName, IEnumerable<Claim> extraClaims, CancellationToken cancellationToken)
    {
        var jwtSection = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSection["SecretKey"]!;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, displayName)
        };
        claims.AddRange(extraClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(int.TryParse(jwtSection["ExpirationInHours"], out var h) ? h : 24),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(tokenString);
    }
}


