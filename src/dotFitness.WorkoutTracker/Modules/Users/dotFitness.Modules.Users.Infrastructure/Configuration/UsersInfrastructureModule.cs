using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using dotFitness.Modules.Users.Domain.Entities;
using dotFitness.Modules.Users.Domain.Repositories;
using dotFitness.Modules.Users.Infrastructure.Repositories;
using dotFitness.Modules.Users.Infrastructure.Handlers;
using dotFitness.Modules.Users.Infrastructure.Settings;
using dotFitness.Modules.Users.Application.Mappers;
using dotFitness.Modules.Users.Application.Commands;
using dotFitness.Modules.Users.Application.Queries;
using dotFitness.Modules.Users.Application.Validators;
using dotFitness.Modules.Users.Application.DTOs;
using dotFitness.SharedKernel.Results;

namespace dotFitness.Modules.Users.Infrastructure.Configuration;

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

        // Register MongoDB collections specific to Users module
        services.AddSingleton(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return database.GetCollection<User>("users");
        });

        services.AddSingleton(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return database.GetCollection<UserMetric>("userMetrics");
        });

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserMetricsRepository, UserMetricsRepository>();

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

        return services;
    }

    /// <summary>
    /// Configures MongoDB indexes for Users module entities
    /// </summary>
    /// <param name="services">The service provider</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task ConfigureUsersModuleIndexes(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        
        // Create indexes for User collection
        var userCollection = database.GetCollection<User>("users");
        var userIndexBuilder = Builders<User>.IndexKeys;
        
        await userCollection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<User>(userIndexBuilder.Ascending(x => x.Email), new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<User>(userIndexBuilder.Ascending(x => x.GoogleId)),
            new CreateIndexModel<User>(userIndexBuilder.Ascending(x => x.CreatedAt)),
            new CreateIndexModel<User>(userIndexBuilder.Ascending(x => x.Roles))
        });

        // Create indexes for UserMetric collection
        var userMetricCollection = database.GetCollection<UserMetric>("userMetrics");
        var userMetricIndexBuilder = Builders<UserMetric>.IndexKeys;
        
        await userMetricCollection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<UserMetric>(userMetricIndexBuilder.Ascending(x => x.UserId)),
            new CreateIndexModel<UserMetric>(userMetricIndexBuilder.Ascending(x => x.Date)),
            new CreateIndexModel<UserMetric>(userMetricIndexBuilder.Ascending(x => x.UserId).Descending(x => x.Date)),
            new CreateIndexModel<UserMetric>(userMetricIndexBuilder.Ascending(x => x.UserId).Ascending(x => x.Date), new CreateIndexOptions { Unique = true })
        });
    }
}
