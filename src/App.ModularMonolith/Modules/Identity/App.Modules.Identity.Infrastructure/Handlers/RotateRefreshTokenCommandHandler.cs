using MediatR;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;
using App.Modules.Identity.Application.DTOs;
using App.Modules.Identity.Application.Abstractions;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class RotateRefreshTokenCommandHandler : IRequestHandler<RotateRefreshTokenCommand, Result<TokenPairDto>>
{
    private readonly IRefreshTokenService _refreshTokenService;

    public RotateRefreshTokenCommandHandler(IRefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }

    public async Task<Result<TokenPairDto>> Handle(RotateRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var rotated = await _refreshTokenService.RotateAsync(request.RefreshToken, cancellationToken);
        if (rotated is null) return Result.Failure<TokenPairDto>("Invalid refresh token");
        return Result.Success(new TokenPairDto { AccessToken = rotated.Value.accessToken, RefreshToken = rotated.Value.refreshToken });
    }
}


