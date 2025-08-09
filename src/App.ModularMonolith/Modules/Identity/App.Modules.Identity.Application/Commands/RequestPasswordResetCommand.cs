using MediatR;
using App.SharedKernel.Results;

namespace App.Modules.Identity.Application.Commands;

public class RequestPasswordResetCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
}


