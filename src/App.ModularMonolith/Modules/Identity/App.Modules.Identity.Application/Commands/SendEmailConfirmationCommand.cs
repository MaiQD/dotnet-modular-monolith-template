using MediatR;
using App.SharedKernel.Results;

namespace App.Modules.Identity.Application.Commands;

public class SendEmailConfirmationCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
}


