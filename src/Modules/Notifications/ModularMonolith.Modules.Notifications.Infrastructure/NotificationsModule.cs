using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Modules.Notifications.Application.Commands.SendNotification;
using ModularMonolith.Modules.Notifications.Application.Queries;
using ModularMonolith.Modules.Notifications.Application.Queries.GetNotification;
using ModularMonolith.Modules.Notifications.Application.Queries.GetNotifications;
using ModularMonolith.Modules.Notifications.Application.Services;
using ModularMonolith.Modules.Notifications.Domain.Repositories;
using ModularMonolith.Modules.Notifications.Infrastructure.Persistence;
using ModularMonolith.Modules.Notifications.Infrastructure.Persistence.Queries;
using ModularMonolith.Modules.Notifications.Infrastructure.Persistence.Repositories;
using ModularMonolith.Modules.Notifications.Infrastructure.Services;
using ModularMonolith.Shared.Abstractions.Kernel;
using ModularMonolith.Shared.Abstractions.Modules;
using ModularMonolith.Shared.Infrastructure.Extensions;

namespace ModularMonolith.Modules.Notifications.Infrastructure;

public sealed class NotificationsModule : IModule
{
    public string Name => "Notifications";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddModuleDbContext<NotificationsDbContext>(configuration, "notifications");
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NotificationsDbContext>());

        // Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Query Services
        services.AddScoped<INotificationsQueryService, NotificationsQueryService>();

        // Notification Senders (stubs for now)
        services.AddScoped<INotificationSender, EmailNotificationSender>();
        services.AddScoped<INotificationSender, SmsNotificationSender>();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SendNotificationCommand).Assembly));
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications")
            .RequireAuthorization()
            .WithTags("Notifications");

        group.MapPost("/send", SendNotification);
        group.MapGet("/{id:guid}", GetNotification);
        group.MapGet("/", GetNotifications);
    }

    public async Task InitializeAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    private static async Task<IResult> SendNotification(
        SendNotificationCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new { notificationId = result.Value })
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetNotification(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetNotificationQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> GetNotifications(
        IMediator mediator,
        int page = 1,
        int pageSize = 10,
        string? type = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationsQuery(page, pageSize, type, status);
        var result = await mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}
