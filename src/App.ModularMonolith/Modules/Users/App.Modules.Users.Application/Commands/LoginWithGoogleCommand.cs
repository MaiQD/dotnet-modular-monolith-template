using MediatR;
using App.Modules.Users.Application.DTOs;
using App.SharedKernel.Results;
using System.ComponentModel.DataAnnotations;

namespace App.Modules.Users.Application.Commands;

public record LoginWithGoogleCommand([Required] string GoogleToken) : IRequest<Result<LoginResponseDto>>;
