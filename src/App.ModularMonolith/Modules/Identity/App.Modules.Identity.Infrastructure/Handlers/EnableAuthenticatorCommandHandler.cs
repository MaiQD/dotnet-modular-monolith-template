using MediatR;
using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;
using App.Modules.Identity.Application.DTOs;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class EnableAuthenticatorCommandHandler : IRequestHandler<EnableAuthenticatorCommand, Result<EnableAuthenticatorResponseDto>>
{
    private readonly UserManager<AppUser> _userManager;

    public EnableAuthenticatorCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<EnableAuthenticatorResponseDto>> Handle(EnableAuthenticatorCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null) return Result.Failure<EnableAuthenticatorResponseDto>("User not found");
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }
        var response = new EnableAuthenticatorResponseDto
        {
            SharedKey = key!,
            AuthenticatorUri = $"otpauth://totp/App:{user.Email}?secret={key}&issuer=App"
        };
        return Result.Success(response);
    }
}


