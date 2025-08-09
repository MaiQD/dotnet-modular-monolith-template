using MediatR;
using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.SharedKernel.Interfaces;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;
using System.Web;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class SendEmailConfirmationCommandHandler : IRequestHandler<SendEmailConfirmationCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly App.Modules.Identity.Application.Abstractions.IIdentityAuditLogger _audit;

    public SendEmailConfirmationCommandHandler(UserManager<AppUser> userManager, IEmailSender emailSender, App.Modules.Identity.Application.Abstractions.IIdentityAuditLogger audit)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _audit = audit;
    }

    public async Task<Result> Handle(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result.Success(); // don't leak existence
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        var link = $"/api/v1/auth/confirm-email?userId={user.Id}&token={encodedToken}";
        await _emailSender.SendAsync(user.Email!, "Confirm your email", $"Click to confirm: {link}", cancellationToken);
        await _audit.LogAsync(user.Id, "EmailConfirmationSent", null, cancellationToken);
        return Result.Success();
    }
}


