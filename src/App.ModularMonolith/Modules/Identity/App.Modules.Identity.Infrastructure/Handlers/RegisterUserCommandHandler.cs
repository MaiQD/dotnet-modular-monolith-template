using MediatR;
using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.Modules.Identity.Domain.Roles;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public RegisterUserCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new AppUser { Id = Guid.NewGuid(), UserName = request.Email, Email = request.Email, DisplayName = request.DisplayName };
        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            var error = string.Join("; ", created.Errors.Select(e => e.Description));
            return Result.Failure(error);
        }
        await _userManager.AddToRoleAsync(user, IdentityRoles.User);
        return Result.Success();
    }
}


