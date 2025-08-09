using MediatR;
using App.SharedKernel.Results;

namespace App.Modules.Identity.Application.Commands;

public class ResetPasswordCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}


