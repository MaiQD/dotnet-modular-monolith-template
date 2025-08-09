using MediatR;
using Microsoft.Extensions.Logging;
using App.Modules.Users.Application.Commands;
using App.Modules.Users.Application.DTOs;
using App.Modules.Users.Application.Mappers;
using App.Modules.Users.Domain.Entities;
using App.Modules.Users.Domain.Repositories;
using App.SharedKernel.Results;

namespace App.Modules.Users.Infrastructure.Handlers;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly UserMapper _userMapper;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        UserMapper userMapper,
        ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _userRepository = userRepository;
        _userMapper = userMapper;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing user
            var userResult = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result.Failure<UserDto>("User not found");
            }

            var user = userResult.Value!;

            // Update user properties
            // Only update display name if it's not empty/whitespace
            if (!string.IsNullOrWhiteSpace(request.DisplayName))
            {
                user.DisplayName = request.DisplayName;
            }
            
            user.DateOfBirth = request.DateOfBirth;
            user.UpdatedAt = DateTime.UtcNow;

            // Save changes
            var updateResult = await _userRepository.UpdateAsync(user, cancellationToken);
            if (updateResult.IsFailure)
            {
                return Result.Failure<UserDto>(updateResult.Error!);
            }

            var updatedUser = updateResult.Value!;
            var userDto = _userMapper.ToDto(updatedUser);

            _logger.LogInformation("User profile updated successfully for user {UserId}", request.UserId);
            return Result.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user profile for user {UserId}", request.UserId);
            return Result.Failure<UserDto>($"Failed to update user profile: {ex.Message}");
        }
    }
}
