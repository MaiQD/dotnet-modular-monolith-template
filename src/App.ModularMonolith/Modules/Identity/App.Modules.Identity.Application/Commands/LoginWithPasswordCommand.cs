using MediatR;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.DTOs;

namespace App.Modules.Identity.Application.Commands;

public class LoginWithPasswordCommand : IRequest<Result<TokenPairDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}


