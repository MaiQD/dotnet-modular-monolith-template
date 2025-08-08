using MediatR;
using App.Modules.Users.Application.DTOs;
using App.SharedKernel.Results;
using System.ComponentModel.DataAnnotations;

namespace App.Modules.Users.Application.Queries;

public record GetUserByIdQuery(string UserId) : IRequest<Result<UserDto>>;
