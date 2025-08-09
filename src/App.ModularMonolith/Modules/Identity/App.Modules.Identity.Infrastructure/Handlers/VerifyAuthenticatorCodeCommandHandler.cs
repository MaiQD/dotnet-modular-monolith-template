using MediatR;
using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class VerifyAuthenticatorCodeCommandHandler : IRequestHandler<VerifyAuthenticatorCodeCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public VerifyAuthenticatorCodeCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(VerifyAuthenticatorCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null) return Result.Failure("User not found");
        var valid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);
        if (!valid) return Result.Failure("Invalid code");
        await _userManager.SetTwoFactorEnabledAsync(user, true);
        return Result.Success();
    }
}


