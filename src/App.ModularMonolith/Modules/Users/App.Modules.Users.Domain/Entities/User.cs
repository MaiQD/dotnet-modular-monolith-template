using App.SharedKernel.Interfaces;

namespace App.Modules.Users.Domain.Entities;

//TODO: only keep necessary properties
public class User : IEntity
{
    //TODO: Ids never using string
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string? GoogleId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsOnboarded { get; set; } = false;

    public DateTime? OnboardingCompletedAt { get; set; }


}
