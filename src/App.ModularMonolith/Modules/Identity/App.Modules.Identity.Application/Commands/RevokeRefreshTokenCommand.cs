using MediatR;
using App.SharedKernel.Results;

namespace App.Modules.Identity.Application.Commands;

public class RevokeRefreshTokenCommand : IRequest<Result>
{
    public string RefreshToken { get; set; } = string.Empty;
}


