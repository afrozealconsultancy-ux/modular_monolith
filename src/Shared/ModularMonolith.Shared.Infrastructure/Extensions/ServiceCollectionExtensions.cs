using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.Messaging;
using ModularMonolith.Shared.Abstractions.Tenancy;
using ModularMonolith.Shared.Abstractions.Time;
using ModularMonolith.Shared.Infrastructure.Auth;
using ModularMonolith.Shared.Infrastructure.EventBus;
using ModularMonolith.Shared.Infrastructure.Tenancy;
using ModularMonolith.Shared.Infrastructure.Time;

namespace ModularMonolith.Shared.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers shared infrastructure services.
    /// </summary>
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Core services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        // MassTransit + RabbitMQ
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqSettings = configuration.GetSection("RabbitMQ");
                cfg.Host(rabbitMqSettings["Host"] ?? "localhost", "/", h =>
                {
                    h.Username(rabbitMqSettings["Username"] ?? "guest");
                    h.Password(rabbitMqSettings["Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }

    /// <summary>
    /// Adds dual Keycloak realm authentication (staff + tenant).
    /// </summary>
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloakStaffSettings = configuration.GetSection("Keycloak:Staff");
        var keycloakTenantSettings = configuration.GetSection("Keycloak:Tenant");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer("Staff", options =>
            {
                options.Authority = keycloakStaffSettings["Authority"];
                options.Audience = keycloakStaffSettings["Audience"];
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddJwtBearer("Tenant", options =>
            {
                options.Authority = keycloakTenantSettings["Authority"];
                options.Audience = keycloakTenantSettings["Audience"];
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Default to Tenant scheme, but allow Staff
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("Tenant", "Staff")
                .Build();
        });

        return services;
    }

    /// <summary>
    /// Registers a module's DbContext with PostgreSQL.
    /// </summary>
    public static IServiceCollection AddModuleDbContext<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string schema)
        where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<TDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema);
            });
        });

        return services;
    }
}
