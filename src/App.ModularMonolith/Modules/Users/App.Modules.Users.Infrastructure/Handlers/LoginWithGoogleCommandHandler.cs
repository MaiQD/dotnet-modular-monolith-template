using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using App.Modules.Users.Application.Commands;
using App.Modules.Users.Application.DTOs;
using App.Modules.Users.Domain.Entities;
using App.Modules.Users.Domain.Repositories;
using App.Modules.Users.Infrastructure.Settings;
using App.SharedKernel.Results;

namespace App.Modules.Users.Infrastructure.Handlers;

public class LoginWithGoogleCommandHandler : IRequestHandler<LoginWithGoogleCommand, Result<LoginResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<LoginWithGoogleCommandHandler> _logger;
    private readonly JwtSettings _jwtSettings;
    private readonly AdminSettings _adminSettings;

    public LoginWithGoogleCommandHandler(
        IUserRepository userRepository,
        ILogger<LoginWithGoogleCommandHandler> logger,
        IOptions<JwtSettings> jwtSettings,
        IOptions<AdminSettings> adminSettings)
    {
        _userRepository = userRepository;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
        _adminSettings = adminSettings.Value;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify Google token
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.GoogleToken);
            if (payload == null)
            {
                return Result.Failure<LoginResponseDto>("Invalid Google token");
            }

            // Check if user exists
            var existingUserResult = await _userRepository.GetByGoogleIdAsync(payload.Subject, cancellationToken);
            User user;

            if (existingUserResult.IsSuccess)
            {
                user = existingUserResult.Value!;
                _logger.LogInformation("Existing user logged in: {Email}", user.Email);
            }
            else
            {
                // Create new user
                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    DisplayName = payload.Name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userRepository.CreateAsync(user, cancellationToken);
                if (createResult.IsFailure)
                {
                    return Result.Failure<LoginResponseDto>(createResult.Error!);
                }

                user = createResult.Value!;
                _logger.LogInformation("New user created: {Email}", user.Email);
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

            var response = new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                ExpiresAt = expiresAt
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Google login for token: {TokenPrefix}", 
                request.GoogleToken[..Math.Min(10, request.GoogleToken.Length)]);
            return Result.Failure<LoginResponseDto>($"Login failed: {ex.Message}");
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new("GoogleId", user.GoogleId ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}