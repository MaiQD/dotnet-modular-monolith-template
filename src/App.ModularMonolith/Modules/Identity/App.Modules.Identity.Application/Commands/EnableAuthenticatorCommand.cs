using MediatR;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.DTOs;

namespace App.Modules.Identity.Application.Commands;

public class EnableAuthenticatorCommand : IRequest<Result<EnableAuthenticatorResponseDto>>
{
    public string UserId { get; set; } = string.Empty;
}


