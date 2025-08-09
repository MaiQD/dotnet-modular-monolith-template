using Microsoft.AspNetCore.Identity;
using App.Modules.Identity.Domain.Entities;
using App.Modules.Identity.Domain.Roles;
using Microsoft.Extensions.DependencyInjection;

namespace App.Modules.Identity.Infrastructure.Configuration;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        var roles = new[] { IdentityRoles.Admin, IdentityRoles.User };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new AppRole { Name = role });
            }
        }
    }
}


