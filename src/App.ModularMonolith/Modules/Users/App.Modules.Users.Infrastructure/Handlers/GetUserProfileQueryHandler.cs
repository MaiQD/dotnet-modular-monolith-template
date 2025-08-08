using MediatR;
using Microsoft.Extensions.Logging;
using App.Modules.Users.Application.Queries;
using App.Modules.Users.Application.DTOs;
using App.Modules.Users.Application.Mappers;
using App.Modules.Users.Domain.Repositories;
using App.SharedKernel.Results;

namespace App.Modules.Users.Infrastructure.Handlers;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly UserMapper _userMapper;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        UserMapper userMapper,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _userRepository = userRepository;
        _userMapper = userMapper;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userResult = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result.Failure<UserDto>("User profile not found");
            }

            var user = userResult.Value!;
            var userDto = _userMapper.ToDto(user);

            return Result.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user profile for user {UserId}", request.UserId);
            return Result.Failure<UserDto>($"Failed to get user profile: {ex.Message}");
        }
    }
}
