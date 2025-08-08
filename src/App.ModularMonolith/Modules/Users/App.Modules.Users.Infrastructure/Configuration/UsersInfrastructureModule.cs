using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using App.Modules.Users.Infrastructure.Persistence;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using App.Modules.Users.Domain.Entities;
using App.Modules.Users.Domain.Repositories;
using App.Modules.Users.Infrastructure.Repositories;
using App.Modules.Users.Infrastructure.Handlers;
using App.Modules.Users.Infrastructure.Settings;
using App.Modules.Users.Application.Mappers;
using App.Modules.Users.Application.Commands;
using App.Modules.Users.Application.Queries;
using App.Modules.Users.Application.Validators;
using App.Modules.Users.Application.DTOs;
using App.SharedKernel.Results;

namespace App.Modules.Users.Infrastructure.Configuration;

/// <summary>
/// Configuration class for Users module infrastructure services
/// </summary>
public static class UsersInfrastructureModule
{
    /// <summary>
    /// Adds all Users module services and configuration to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration to bind settings from</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
    // Database: EF Core + PostgreSQL
    var connectionString = configuration.GetConnectionString("PostgreSQL")
                   ?? throw new InvalidOperationException("PostgreSQL connection string is not configured");
    services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(connectionString));

        // Configure User Module Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<AdminSettings>(configuration.GetSection("AdminSettings"));

        // Configure JWT Authentication (since it's primarily used for user authentication)
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

    // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Register MediatR handlers
        services.AddScoped<IRequestHandler<LoginWithGoogleCommand, Result<LoginResponseDto>>, LoginWithGoogleCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateUserProfileCommand, Result<UserDto>>, UpdateUserProfileCommandHandler>();
        services.AddScoped<IRequestHandler<GetUserByIdQuery, Result<UserDto>>, GetUserByIdQueryHandler>();
        services.AddScoped<IRequestHandler<GetUserProfileQuery, Result<UserDto>>, GetUserProfileQueryHandler>();

        // Register validators
        services.AddScoped<IValidator<LoginWithGoogleCommand>, LoginWithGoogleCommandValidator>();
        services.AddScoped<IValidator<UpdateUserProfileCommand>, UpdateUserProfileCommandValidator>();

        // Register Mapperly mappers - they will be generated as implementations
        services.AddScoped<UserMapper>();

        return services;
    }

    // With EF Core, indexes are configured via entity configurations and migrations.
    public static Task ConfigureUsersModuleIndexes(IServiceProvider services) => Task.CompletedTask;
}
