using Riok.Mapperly.Abstractions;
using App.Modules.Users.Domain.Entities;
using App.Modules.Users.Application.DTOs;

namespace App.Modules.Users.Application.Mappers;

[Mapper]
public partial class UserMapper
{
    // Ignore IsOnboarded and OnboardingCompletedAt for now
    [MapperIgnoreSource(nameof(User.IsOnboarded))]
    [MapperIgnoreSource(nameof(User.OnboardingCompletedAt))]
    public partial UserDto ToDto(User user);
}
