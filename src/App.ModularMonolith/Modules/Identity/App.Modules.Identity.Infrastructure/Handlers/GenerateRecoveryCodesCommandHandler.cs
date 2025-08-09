using MediatR;
using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;
using App.Modules.Identity.Application.DTOs;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class GenerateRecoveryCodesCommandHandler : IRequestHandler<GenerateRecoveryCodesCommand, Result<RecoveryCodesDto>>
{
    private readonly UserManager<AppUser> _userManager;

    public GenerateRecoveryCodesCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<RecoveryCodesDto>> Handle(GenerateRecoveryCodesCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null) return Result.Failure<RecoveryCodesDto>("User not found");
        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, request.Count);
        return Result.Success(new RecoveryCodesDto { Codes = codes });
    }
}


