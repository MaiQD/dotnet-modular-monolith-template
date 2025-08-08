using Riok.Mapperly.Abstractions;
using App.Modules.Users.Domain.Entities;
using App.Modules.Users.Application.DTOs;

namespace App.Modules.Users.Application.Mappers;

[Mapper]
public partial class UserMapper
{
    public partial UserDto ToDto(User user);
    
    // Custom mapping for enum conversions
}
