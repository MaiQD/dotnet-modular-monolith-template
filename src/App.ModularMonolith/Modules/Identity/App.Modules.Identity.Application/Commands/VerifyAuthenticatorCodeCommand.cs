using MediatR;
using App.SharedKernel.Results;

namespace App.Modules.Identity.Application.Commands;

public class VerifyAuthenticatorCodeCommand : IRequest<Result>
{
    public string UserId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}


