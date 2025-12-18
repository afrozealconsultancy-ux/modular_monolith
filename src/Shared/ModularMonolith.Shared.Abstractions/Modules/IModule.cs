using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Shared.Abstractions.Modules;

/// <summary>
/// Interface for defining a module in the modular monolith.
/// Each module should implement this interface to register its services and endpoints.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Registers module services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Maps module endpoints.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    void MapEndpoints(IEndpointRouteBuilder endpoints);

    /// <summary>
    /// Initializes the module (e.g., run migrations, seed data).
    /// </summary>
    /// <param name="app">The application builder.</param>
    Task InitializeAsync(IApplicationBuilder app);
}
