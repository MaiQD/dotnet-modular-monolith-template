using MediatR;
using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;
using App.Modules.Identity.Application.DTOs;
using App.Modules.Identity.Application.Abstractions;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class LoginWithPasswordCommandHandler : IRequestHandler<LoginWithPasswordCommand, Result<TokenPairDto>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IIdentityAuditLogger _audit;

    public LoginWithPasswordCommandHandler(UserManager<AppUser> userManager, IRefreshTokenService refreshTokenService, IIdentityAuditLogger audit)
    {
        _userManager = userManager;
        _refreshTokenService = refreshTokenService;
        _audit = audit;
    }

    public async Task<Result<TokenPairDto>> Handle(LoginWithPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            await _audit.LogAsync(null, "LoginFailed", "User not found", cancellationToken);
            return Result.Failure<TokenPairDto>("Invalid credentials");
        }
        var ok = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!ok)
        {
            await _audit.LogAsync(user.Id, "LoginFailed", "Invalid password", cancellationToken);
            return Result.Failure<TokenPairDto>("Invalid credentials");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var extraClaims = roles.Select(r => new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, r));
        var (access, refresh) = await _refreshTokenService.IssueAsync(user.Id, user.Email!, user.DisplayName, extraClaims, cancellationToken);
        await _audit.LogAsync(user.Id, "LoginSuccess", null, cancellationToken);
        return Result.Success(new TokenPairDto { AccessToken = access, RefreshToken = refresh });
    }
}


