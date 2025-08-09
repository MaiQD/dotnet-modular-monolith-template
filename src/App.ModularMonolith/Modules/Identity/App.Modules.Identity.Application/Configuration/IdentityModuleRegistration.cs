using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Modules.Identity.Application.Configuration;

public static class IdentityModuleRegistration
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        var infrastructureAssembly = System.AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "App.Modules.Identity.Infrastructure")
            ?? System.Reflection.Assembly.Load("App.Modules.Identity.Infrastructure");

        var moduleType = infrastructureAssembly.GetType("App.Modules.Identity.Infrastructure.Configuration.IdentityInfrastructureModule");
        var addModuleMethod = moduleType?.GetMethod("AddIdentityModule", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException("Could not find AddIdentityModule in Identity Infrastructure layer");

        addModuleMethod.Invoke(null, new object[] { services, configuration });
        return services;
    }
}


