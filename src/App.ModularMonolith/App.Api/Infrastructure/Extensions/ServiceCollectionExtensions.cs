using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using FluentValidation.AspNetCore;
using App.SharedKernel.Outbox;
using App.Api.Infrastructure.Settings;
using App.Api.Infrastructure.Swagger;
 

namespace App.Api.Infrastructure.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to organize service registrations
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core API services including controllers, versioning, and validation
    /// </summary>
    public static IServiceCollection AddCoreApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        
        services.AddApiVersioning(opt =>
        {
            opt.DefaultApiVersion = new ApiVersion(1, 0);
            opt.AssumeDefaultVersionWhenUnspecified = true;
        });

        services.AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters();

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserOnly", policy => policy.RequireRole("User", "Admin"));
        });
        return services;
    }

    /// <summary>
    /// Adds Swagger/OpenAPI services with OAuth2 and Bearer token authentication
    /// </summary>
    public static IServiceCollection AddSwaggerWithOAuth(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "App API", Version = "v1" });
            
            // Add Bearer token security definition
            c.AddSecurityDefinition("Bearer", new()
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid JWT token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            
            // Add OAuth2 security definition for Google
            c.AddSecurityDefinition("OAuth2", new()
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new()
                {
                    AuthorizationCode = new()
                    {
                        AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
                        TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID Connect" },
                            { "email", "Email address" },
                            { "profile", "Profile information" }
                        }
                    }
                },
                Description = "Google OAuth2 authentication"
            });
            
            // Add security requirements
            c.AddSecurityRequirement(new()
            {
                {
                    new()
                    {
                        Reference = new()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[]{}
                },
                {
                    new()
                    {
                        Reference = new()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "OAuth2"
                        }
                    },
                    ["openid", "email", "profile"]
                }
            });
            
            // Add operation filter to apply security to specific endpoints
            c.OperationFilter<SecurityRequirementsOperationFilter>();
        });

        return services;
    }

    /// <summary>
    /// Adds CORS configuration
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        return services;
    }

    

    /// <summary>
    /// Adds module-related services including health checks and MediatR
    /// </summary>
    public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        // Add health checks for modules
        services.AddModuleHealthChecks();

        // Set up module registry logger
        ModuleRegistry.SetLogger(logger);

        // Validate module configuration
        var configurationValidation = ModuleConfigurationValidator.ValidateModuleConfiguration(configuration, logger);
        if (!configurationValidation.IsValid)
        {
            logger.LogWarning("Module configuration validation found issues: {ValidationResult}", configurationValidation.ToJson());
        }

        // Register all modules automatically
        ModuleRegistry.RegisterAllModules(services, configuration);

        // Add MediatR with automatic module assembly discovery
        services.AddMediatR(cfg => 
        {
            // Register API assembly
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            
            // Auto-discover and register all module assemblies
            ModuleRegistry.RegisterModuleAssemblies(cfg);
        });

        return services;
    }
} 