using MediatR;
using App.SharedKernel.Results;

namespace App.Modules.Identity.Application.Commands;

public class ConfirmEmailCommand : IRequest<Result>
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}


