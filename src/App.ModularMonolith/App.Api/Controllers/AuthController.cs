using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.Modules.Identity.Application.Abstractions;
using App.Modules.Identity.Application.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace App.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")] 
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IIdentityService _identityService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthController(UserManager<AppUser> userManager, IIdentityService identityService, IRefreshTokenService refreshTokenService)
    {
        _userManager = userManager;
        _identityService = identityService;
        _refreshTokenService = refreshTokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
    {
        var user = new AppUser { Id = Guid.NewGuid(), UserName = request.Email, Email = request.Email, DisplayName = request.DisplayName };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        await _userManager.AddToRoleAsync(user, App.Modules.Identity.Domain.Roles.IdentityRoles.User);
        return Ok();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Unauthorized();
        var correct = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!correct) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var extraClaims = roles.Select(r => new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, r));
        var (accessToken, refreshToken) = await _refreshTokenService.IssueAsync(user.Id, user.Email!, user.DisplayName, extraClaims, ct);
        return Ok(new { accessToken, refreshToken });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody, BindRequired] string refreshToken, CancellationToken ct)
    {
        var rotated = await _refreshTokenService.RotateAsync(refreshToken, ct);
        if (rotated is null) return Unauthorized();
        return Ok(new { accessToken = rotated.Value.accessToken, refreshToken = rotated.Value.refreshToken });
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody, BindRequired] string refreshToken, CancellationToken ct)
    {
        await _refreshTokenService.RevokeAsync(refreshToken, ct);
        return NoContent();
    }
}


