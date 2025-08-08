using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
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
using App.Modules.Users.Application.Abstractions;
using App.SharedKernel.Results;
using App.SharedKernel.Inbox;

namespace App.Modules.Users.Infrastructure.Installers;

/// <summary>
/// Implementation of IUsersModuleInstaller that registers all Users module services
/// </summary>
public class UsersModuleInstaller : IUsersModuleInstaller
{
    public IServiceCollection Install(IServiceCollection services, IConfiguration configuration)
    {
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
        services.AddScoped<IUserMetricsRepository, UserMetricsRepository>();

    // Inbox repository is not wired in this template for EF Core; add as needed.

        // Register MediatR handlers
        services.AddScoped<IRequestHandler<LoginWithGoogleCommand, Result<LoginResponseDto>>, LoginWithGoogleCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateUserProfileCommand, Result<UserDto>>, UpdateUserProfileCommandHandler>();
        services.AddScoped<IRequestHandler<AddUserMetricCommand, Result<UserMetricDto>>, AddUserMetricCommandHandler>();
        services.AddScoped<IRequestHandler<GetUserByIdQuery, Result<UserDto>>, GetUserByIdQueryHandler>();
        services.AddScoped<IRequestHandler<GetUserProfileQuery, Result<UserDto>>, GetUserProfileQueryHandler>();
        services.AddScoped<IRequestHandler<GetLatestUserMetricQuery, Result<UserMetricDto>>, GetLatestUserMetricQueryHandler>();
        services.AddScoped<IRequestHandler<GetUserMetricsQuery, Result<IEnumerable<UserMetricDto>>>, GetUserMetricsQueryHandler>();

        // Register validators
        services.AddScoped<IValidator<LoginWithGoogleCommand>, LoginWithGoogleCommandValidator>();
        services.AddScoped<IValidator<UpdateUserProfileCommand>, UpdateUserProfileCommandValidator>();
        services.AddScoped<IValidator<AddUserMetricCommand>, AddUserMetricCommandValidator>();

        // Register Mapperly mappers - they will be generated as implementations
        services.AddScoped<UserMapper>();
        services.AddScoped<UserMetricMapper>();

        // Register the installer itself so it can be resolved
        services.AddSingleton<IUsersModuleInstaller, UsersModuleInstaller>();

        return services;
    }

    public Task ConfigureIndexes(IServiceProvider services) => Task.CompletedTask;
}
