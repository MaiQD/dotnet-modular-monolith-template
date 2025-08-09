using MediatR;
using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly App.Modules.Identity.Application.Abstractions.IIdentityAuditLogger _audit;

    public ResetPasswordCommandHandler(UserManager<AppUser> userManager, App.Modules.Identity.Application.Abstractions.IIdentityAuditLogger audit)
    {
        _userManager = userManager;
        _audit = audit;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result.Failure("Invalid reset request");
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (result.Succeeded)
        {
            await _audit.LogAsync(user.Id, "PasswordReset", null, cancellationToken);
            return Result.Success();
        }
        return Result.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}


