using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Shared.Abstractions.Modules;
using ModularMonolith.Shared.Abstractions.Persistence;
using ModularMonolith.Shared.Infrastructure.Extensions;
using ModularMonolith.Modules.Customers.Application.Commands.CreateCustomer;
using ModularMonolith.Modules.Customers.Application.Commands.UpdateCustomer;
using ModularMonolith.Modules.Customers.Application.Commands.DeleteCustomer;
using ModularMonolith.Modules.Customers.Application.Queries.GetCustomer;
using ModularMonolith.Modules.Customers.Application.Queries.GetCustomers;
using ModularMonolith.Modules.Customers.Domain.Repositories;
using ModularMonolith.Modules.Customers.Infrastructure.Persistence;
using ModularMonolith.Modules.Customers.Infrastructure.Repositories;
using ModularMonolith.Modules.Customers.Infrastructure.Services;

namespace ModularMonolith.Modules.Customers.Infrastructure;

/// <summary>
/// Customers module registration.
/// This is the ONLY public class in Infrastructure - the entry point for the module.
/// </summary>
public sealed class CustomersModule : IModule
{
    public string Name => "Customers";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddModuleDbContext<CustomersDbContext>(configuration, "customers");

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CustomersDbContext>());

        // Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Query Services
        services.AddScoped<ICustomersQueryService, CustomersQueryService>();

        // MediatR handlers from Application assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly);
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(CreateCustomerCommand).Assembly);

        // MassTransit consumers (if any event handlers in this module)
        // services.AddMassTransit(x => { ... });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/customers")
            .RequireAuthorization()
            .WithTags("Customers")
            .WithOpenApi();

        // POST /api/customers - Create customer
        group.MapPost("/", CreateCustomer)
            .WithName("CreateCustomer")
            .WithSummary("Create a new customer")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // GET /api/customers/{id} - Get customer by ID
        group.MapGet("/{id:guid}", GetCustomer)
            .WithName("GetCustomer")
            .WithSummary("Get customer by ID")
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // GET /api/customers - Get customers (paginated)
        group.MapGet("/", GetCustomers)
            .WithName("GetCustomers")
            .WithSummary("Get customers with pagination")
            .Produces<PagedList<CustomerListItemDto>>(StatusCodes.Status200OK);

        // PUT /api/customers/{id} - Update customer
        group.MapPut("/{id:guid}", UpdateCustomer)
            .WithName("UpdateCustomer")
            .WithSummary("Update customer")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // DELETE /api/customers/{id} - Delete customer (soft delete)
        group.MapDelete("/{id:guid}", DeleteCustomer)
            .WithName("DeleteCustomer")
            .WithSummary("Delete customer")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    public async Task InitializeAsync(IApplicationBuilder app)
    {
        // Run migrations on startup
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    // Endpoint handlers
    private static async Task<IResult> CreateCustomer(
        [FromBody] CreateCustomerCommand command,
        ISender sender,
        ICurrentUser currentUser)
    {
        var result = await sender.Send(command);

        return result.IsSuccess
            ? Results.Created($"/api/customers/{result.Value}", new { id = result.Value })
            : Results.BadRequest(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description,
                Status = StatusCodes.Status400BadRequest
            });
    }

    private static async Task<IResult> GetCustomer(
        Guid id,
        ISender sender)
    {
        var query = new GetCustomerQuery(id);
        var result = await sender.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description,
                Status = StatusCodes.Status404NotFound
            });
    }

    private static async Task<IResult> GetCustomers(
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        ISender sender = null!)
    {
        var query = new GetCustomersQuery(searchTerm, page, pageSize);
        var result = await sender.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description
            });
    }

    private static async Task<IResult> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        ISender sender)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.Name,
            request.Email,
            request.PhoneNumber);

        var result = await sender.Send(command);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description
            });
    }

    private static async Task<IResult> DeleteCustomer(
        Guid id,
        ISender sender)
    {
        var command = new DeleteCustomerCommand(id);
        var result = await sender.Send(command);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description
            });
    }

    // Request DTOs for endpoints
    private record UpdateCustomerRequest(
        string Name,
        string Email,
        string? PhoneNumber);
}

// Helper extension to register FluentValidation
file static class ValidationExtensions
{
    public static IServiceCollection AddValidatorsFromAssembly(
        this IServiceCollection services,
        System.Reflection.Assembly assembly)
    {
        // FluentValidation auto-registration would go here
        // For now, validators are registered via MediatR pipeline behaviors
        return services;
    }
}
