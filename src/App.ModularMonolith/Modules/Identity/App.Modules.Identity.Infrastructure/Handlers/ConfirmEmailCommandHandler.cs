using MediatR;
using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly App.Modules.Identity.Application.Abstractions.IIdentityAuditLogger _audit;

    public ConfirmEmailCommandHandler(UserManager<AppUser> userManager, App.Modules.Identity.Application.Abstractions.IIdentityAuditLogger audit)
    {
        _userManager = userManager;
        _audit = audit;
    }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null) return Result.Failure("Invalid confirmation request");
        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (result.Succeeded)
        {
            await _audit.LogAsync(Guid.Parse(request.UserId), "EmailConfirmed", null, cancellationToken);
            return Result.Success();
        }
        return Result.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}


