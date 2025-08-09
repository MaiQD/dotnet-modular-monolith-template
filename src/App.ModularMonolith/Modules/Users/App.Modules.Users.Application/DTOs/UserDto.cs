using System.ComponentModel.DataAnnotations;

namespace App.Modules.Users.Application.DTOs;

public class UserDto
{
    
    [Required]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string DisplayName { get; set; } = string.Empty;
    
    public string? GoogleId { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}
