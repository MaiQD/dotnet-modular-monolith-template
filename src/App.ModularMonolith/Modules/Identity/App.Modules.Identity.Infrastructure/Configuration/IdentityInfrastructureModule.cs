using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using App.Modules.Identity.Infrastructure;
using App.Modules.Identity.Application.Abstractions;
using App.Modules.Identity.Domain.Entities;

namespace App.Modules.Identity.Infrastructure.Configuration;

public static class IdentityInfrastructureModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("PostgreSQL connection string is not configured");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
        );

        services.AddIdentityCore<AppUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<IdentityDbContext>();

        // JWT
        var jwtSection = configuration.GetSection("JwtSettings");
        var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole(App.Modules.Identity.Domain.Roles.IdentityRoles.Admin));
            options.AddPolicy("UserOnly", policy => policy.RequireRole(App.Modules.Identity.Domain.Roles.IdentityRoles.User, App.Modules.Identity.Domain.Roles.IdentityRoles.Admin));
        });

        services.AddScoped<IIdentityService, JwtIdentityService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        return services;
    }
}


