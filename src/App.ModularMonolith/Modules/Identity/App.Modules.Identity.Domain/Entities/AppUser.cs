using Microsoft.AspNetCore.Identity;

namespace App.Modules.Identity.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}


