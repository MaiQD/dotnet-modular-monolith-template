using MediatR;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.DTOs;

namespace App.Modules.Identity.Application.Commands;

public class RotateRefreshTokenCommand : IRequest<Result<TokenPairDto>>
{
    public string RefreshToken { get; set; } = string.Empty;
}


