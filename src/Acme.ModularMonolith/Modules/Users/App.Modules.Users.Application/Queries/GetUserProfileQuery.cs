using MediatR;
using App.Modules.Users.Application.DTOs;
using App.SharedKernel.Results;
using System.ComponentModel.DataAnnotations;

namespace App.Modules.Users.Application.Queries;

public class GetUserProfileQuery : IRequest<Result<UserDto>>
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}
