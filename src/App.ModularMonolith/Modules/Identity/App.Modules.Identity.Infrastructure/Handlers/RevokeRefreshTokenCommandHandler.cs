using MediatR;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.Commands;
using App.Modules.Identity.Application.Abstractions;

namespace App.Modules.Identity.Infrastructure.Handlers;

public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, Result>
{
    private readonly IRefreshTokenService _refreshTokenService;

    public RevokeRefreshTokenCommandHandler(IRefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }

    public async Task<Result> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        await _refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        return Result.Success();
    }
}


