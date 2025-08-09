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

    [HttpPost("send-confirmation")]
    [AllowAnonymous]
    public async Task<IActionResult> SendEmailConfirmation([FromBody] string email, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendEmailConfirmationCommand { Email = email }, ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token, CancellationToken ct)
    {
        var result = await _mediator.Send(new ConfirmEmailCommand { UserId = userId, Token = token }, ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("request-password-reset")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] string email, CancellationToken ct)
    {
        var result = await _mediator.Send(new RequestPasswordResetCommand { Email = email }, ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    public class ResetPasswordRequest { public string Email { get; set; } = string.Empty; public string Token { get; set; } = string.Empty; public string NewPassword { get; set; } = string.Empty; }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResetPasswordCommand { Email = request.Email, Token = request.Token, NewPassword = request.NewPassword }, ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<IActionResult> EnableAuthenticator(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var result = await _mediator.Send(new EnableAuthenticatorCommand { UserId = userId }, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    public class Verify2FaRequest { public string Code { get; set; } = string.Empty; }

    [HttpPost("2fa/verify")]
    [Authorize]
    public async Task<IActionResult> VerifyAuthenticator([FromBody] Verify2FaRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var result = await _mediator.Send(new VerifyAuthenticatorCodeCommand { UserId = userId, Code = request.Code }, ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("2fa/recovery-codes")]
    [Authorize]
    public async Task<IActionResult> GenerateRecoveryCodes(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var result = await _mediator.Send(new GenerateRecoveryCodesCommand { UserId = userId }, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
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


