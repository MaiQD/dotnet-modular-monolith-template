using MediatR;
using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.SharedKernel.Interfaces;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;
using System.Web;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly App.Modules.Identity.Application.Abstractions.IIdentityAuditLogger _audit;

    public RequestPasswordResetCommandHandler(UserManager<AppUser> userManager, IEmailSender emailSender, App.Modules.Identity.Application.Abstractions.IIdentityAuditLogger audit)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _audit = audit;
    }

    public async Task<Result> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result.Success();
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        var link = $"/api/v1/auth/reset-password?email={HttpUtility.UrlEncode(user.Email)}&token={encodedToken}&newPassword={{newPassword}}";
        await _emailSender.SendAsync(user.Email!, "Reset your password", $"Use the following link (update newPassword): {link}", cancellationToken);
        await _audit.LogAsync(user.Id, "PasswordResetRequested", null, cancellationToken);
        return Result.Success();
    }
}


