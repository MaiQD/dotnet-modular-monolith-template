using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using App.Modules.Identity.Application.DTOs;
using App.Modules.Identity.Application.Commands;

namespace App.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")] 
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
    {
        var command = new RegisterUserCommand { Email = request.Email, Password = request.Password, DisplayName = request.DisplayName };
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginWithPasswordCommand { Email = request.Email, Password = request.Password }, ct);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken, CancellationToken ct)
    {
        var result = await _mediator.Send(new RotateRefreshTokenCommand { RefreshToken = refreshToken }, ct);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] string refreshToken, CancellationToken ct)
    {
        var result = await _mediator.Send(new RevokeRefreshTokenCommand { RefreshToken = refreshToken }, ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}


