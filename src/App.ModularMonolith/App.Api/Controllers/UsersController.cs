using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Security.Claims;
using App.Modules.Users.Application.Commands;
using App.Modules.Users.Application.Queries;
using App.Modules.Users.Application.DTOs;
using App.SharedKernel.Results;

namespace App.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's profile information
    /// </summary>
    /// <returns>User profile data</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "User ID not found in token" });
            }

            var query = new GetUserProfileQuery { UserId = userId };
            var result = await _mediator.Send(query);

            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to get profile for user {UserId}: {Error}", userId, result.Error);
                return NotFound(new { error = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting user profile");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An internal server error occurred" });
        }
    }

    /// <summary>
    /// Updates the current user's profile information
    /// </summary>
    /// <param name="command">The profile update data</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "User ID not found in token" });
            }

            // Set the user ID from the token to ensure user can only update their own profile
            command.UserId = userId;

            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to update profile for user {UserId}: {Error}", userId, result.Error);
                
                if (result.Error!.Contains("not found"))
                    return NotFound(new { error = result.Error });
                
                return BadRequest(new { error = result.Error });
            }

            _logger.LogInformation("Profile updated successfully for user {UserId}", userId);
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating user profile");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An internal server error occurred" });
        }
    }
}
