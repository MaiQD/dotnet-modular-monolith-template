using MediatR;
using App.SharedKernel.Results;
using App.Modules.Identity.Application.DTOs;

namespace App.Modules.Identity.Application.Commands;

public class GenerateRecoveryCodesCommand : IRequest<Result<RecoveryCodesDto>>
{
    public string UserId { get; set; } = string.Empty;
    public int Count { get; set; } = 10;
}


