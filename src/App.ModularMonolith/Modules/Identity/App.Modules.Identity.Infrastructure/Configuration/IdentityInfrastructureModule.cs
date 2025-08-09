using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
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
            o.Password.RequireDigit = true;
            o.Password.RequireLowercase = true;
            o.Password.RequireUppercase = false;
            o.Password.RequireNonAlphanumeric = false;
            o.Password.RequiredLength = 6;
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            o.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<IdentityDbContext>();

        services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(opt =>
        {
            opt.SignIn.RequireConfirmedEmail = true;
        });

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

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
                        var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        var tokenStamp = context.Principal?.FindFirst("sstamp")?.Value;
                        if (userId is null || tokenStamp is null)
                        {
                            context.Fail("Invalid token claims");
                            return;
                        }
                        var user = await userManager.FindByIdAsync(userId);
                        if (user is null || !string.Equals(user.SecurityStamp, tokenStamp, StringComparison.Ordinal))
                        {
                            context.Fail("Security stamp validation failed");
                        }
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole(App.Modules.Identity.Domain.Roles.IdentityRoles.Admin));
            options.AddPolicy("UserOnly", policy => policy.RequireRole(App.Modules.Identity.Domain.Roles.IdentityRoles.User, App.Modules.Identity.Domain.Roles.IdentityRoles.Admin));
        });

        services.AddScoped<IIdentityService, JwtIdentityService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<App.SharedKernel.Interfaces.IEmailSender, LoggingEmailSender>();
        services.AddScoped<App.Modules.Identity.Application.Abstractions.IIdentityAuditLogger, IdentityAuditLogger>();

        return services;
    }
}


