using MediatR;
using ModularMonolith.Modules.Notifications.Application.Services;
using ModularMonolith.Modules.Notifications.Contracts.Events;
using ModularMonolith.Modules.Notifications.Domain.Entities;
using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Modules.Notifications.Domain.Repositories;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.CQRS;
using ModularMonolith.Shared.Abstractions.Events;
using ModularMonolith.Shared.Abstractions.Kernel;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Notifications.Application.Commands.SendNotification;

internal sealed class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Result<Guid>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserNotificationService _userNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly IEventBus _eventBus;

    public SendNotificationCommandHandler(
        INotificationRepository notificationRepository,
        IUserNotificationService userNotificationService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ITenantContext tenantContext,
        IEventBus eventBus)
    {
        _notificationRepository = notificationRepository;
        _userNotificationService = userNotificationService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _eventBus = eventBus;
    }

    public async Task<Result<Guid>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        // Validate permissions based on scope
        var permissionCheck = ValidatePermissions(request.Scope);
        if (permissionCheck.IsFailure)
            return Result.Failure<Guid>(permissionCheck.Error);

        // Create notification based on scope
        var notification = await CreateNotificationAsync(request, cancellationToken);
        if (notification.IsFailure)
            return Result.Failure<Guid>(notification.Error);

        // Resolve and add recipients
        var recipientsResult = await ResolveRecipientsAsync(notification.Value, request, cancellationToken);
        if (recipientsResult.IsFailure)
            return Result.Failure<Guid>(recipientsResult.Error);

        // Schedule if requested
        if (request.ScheduledFor.HasValue)
        {
            notification.Value.ScheduleFor(request.ScheduledFor.Value);
        }

        // Mark as processed
        notification.Value.MarkAsProcessed();

        await _notificationRepository.AddAsync(notification.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(new NotificationCreatedIntegrationEvent
        {
            NotificationId = notification.Value.Id,
            Scope = notification.Value.Scope.ToString(),
            Type = notification.Value.Type.ToString(),
            RecipientCount = notification.Value.Recipients.Count,
            CreatedAt = notification.Value.CreatedAt,
            ScheduledFor = notification.Value.ScheduledFor
        }, cancellationToken);

        return Result.Success(notification.Value.Id);
    }

    private Result ValidatePermissions(NotificationScope scope)
    {
        switch (scope)
        {
            case NotificationScope.Personal:
            case NotificationScope.Tenant:
                if (!_currentUser.HasPermission(Permissions.Notifications.Send))
                {
                    return Result.Failure(Error.Forbidden(
                        "Notifications.Send.Forbidden",
                        "You don't have permission to send notifications"));
                }

                if (!_tenantContext.HasTenant)
                {
                    return Result.Failure(Error.Unauthorized(
                        "Notifications.Send.NoTenant",
                        "Tenant context is required for personal/tenant notifications"));
                }
                break;

            case NotificationScope.SelectedTenants:
            case NotificationScope.AllTenants:
                if (!_currentUser.HasPermission(Permissions.Notifications.SendCrossTenant))
                {
                    return Result.Failure(Error.Forbidden(
                        "Notifications.SendCrossTenant.Forbidden",
                        "You don't have permission to send cross-tenant notifications"));
                }
                break;
        }

        return Result.Success();
    }

    private async Task<Result<Notification>> CreateNotificationAsync(
        SendNotificationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            Notification notification = request.Scope switch
            {
                NotificationScope.Personal => Notification.CreatePersonal(
                    request.Type,
                    request.Subject,
                    request.Content,
                    _tenantContext.TenantId!.Value,
                    _currentUser.UserId),

                NotificationScope.Tenant => Notification.CreateTenantBroadcast(
                    request.Type,
                    request.Subject,
                    request.Content,
                    _tenantContext.TenantId!.Value,
                    _currentUser.UserId),

                NotificationScope.SelectedTenants => Notification.CreateForSelectedTenants(
                    request.Type,
                    request.Subject,
                    request.Content,
                    request.TargetTenantIds ?? new List<Guid>(),
                    _currentUser.UserId),

                NotificationScope.AllTenants => Notification.CreateAllTenantsBroadcast(
                    request.Type,
                    request.Subject,
                    request.Content,
                    _currentUser.UserId),

                _ => throw new ArgumentOutOfRangeException(nameof(request.Scope))
            };

            return Result.Success(notification);
        }
        catch (Exception ex)
        {
            return Result.Failure<Notification>(Error.Validation(
                "Notifications.Create.Failed",
                ex.Message));
        }
    }

    private async Task<Result> ResolveRecipientsAsync(
        Notification notification,
        SendNotificationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            switch (request.Scope)
            {
                case NotificationScope.Personal:
                    if (request.TargetUserIds == null || request.TargetUserIds.Count == 0)
                    {
                        return Result.Failure(Error.Validation(
                            "Notifications.Recipients.Required",
                            "Target users are required for personal notifications"));
                    }

                    foreach (var userId in request.TargetUserIds)
                    {
                        var address = await _userNotificationService.GetUserNotificationAddressAsync(
                            userId,
                            _tenantContext.TenantId!.Value,
                            cancellationToken);

                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            notification.AddRecipient(userId, _tenantContext.TenantId!.Value, address);
                        }
                    }
                    break;

                case NotificationScope.Tenant:
                    var tenantUsers = await _userNotificationService.GetTenantUsersAsync(
                        _tenantContext.TenantId!.Value,
                        cancellationToken);

                    var tenantRecipients = tenantUsers.Select(u => (
                        u.UserId,
                        _tenantContext.TenantId!.Value,
                        u.Address
                    ));

                    notification.AddRecipients(tenantRecipients);
                    break;

                case NotificationScope.SelectedTenants:
                    if (request.TargetTenantIds == null || request.TargetTenantIds.Count == 0)
                    {
                        return Result.Failure(Error.Validation(
                            "Notifications.TargetTenants.Required",
                            "Target tenants are required for selected tenants scope"));
                    }

                    var selectedTenantUsers = await _userNotificationService.GetUsersFromTenantsAsync(
                        request.TargetTenantIds,
                        cancellationToken);

                    notification.AddRecipients(selectedTenantUsers);
                    break;

                case NotificationScope.AllTenants:
                    var allUsers = await _userNotificationService.GetAllUsersAsync(cancellationToken);
                    notification.AddRecipients(allUsers);
                    break;
            }

            if (notification.Recipients.Count == 0)
            {
                return Result.Failure(Error.Validation(
                    "Notifications.Recipients.None",
                    "No valid recipients found for this notification"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure(
                "Notifications.Recipients.ResolutionFailed",
                $"Failed to resolve recipients: {ex.Message}"));
        }
    }
}

public static class Permissions
{
    public static class Notifications
    {
        public const string View = "notifications.view";
        public const string Send = "notifications.send";
        public const string SendCrossTenant = "notifications.send.cross-tenant";
        public const string MarkAsRead = "notifications.mark-as-read";
    }
}
