using MediatR;
using ModularMonolith.Modules.Notifications.Application.Services;
using ModularMonolith.Modules.Notifications.Contracts.Events;
using ModularMonolith.Modules.Notifications.Domain.Entities;
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
    private readonly IEnumerable<INotificationSender> _notificationSenders;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly IEventBus _eventBus;

    public SendNotificationCommandHandler(
        INotificationRepository notificationRepository,
        IEnumerable<INotificationSender> notificationSenders,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ITenantContext tenantContext,
        IEventBus eventBus)
    {
        _notificationRepository = notificationRepository;
        _notificationSenders = notificationSenders;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _eventBus = eventBus;
    }

    public async Task<Result<Guid>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Notifications.Send))
        {
            return Result.Failure<Guid>(Error.Forbidden(
                "Notifications.Send.Forbidden",
                "You don't have permission to send notifications"));
        }

        // Ensure tenant context
        if (!_tenantContext.HasTenant)
        {
            return Result.Failure<Guid>(Error.Unauthorized(
                "Notifications.Send.NoTenant",
                "Tenant context is required"));
        }

        // Create notification
        var notification = Notification.Create(
            _tenantContext.TenantId!.Value,
            request.Type,
            request.Recipient,
            request.Subject,
            request.Content);

        await _notificationRepository.AddAsync(notification, cancellationToken);

        // Find appropriate sender
        var sender = _notificationSenders.FirstOrDefault(s => s.Type == request.Type);

        if (sender == null)
        {
            notification.MarkAsFailed($"No sender found for notification type {request.Type}");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<Guid>(Error.Validation(
                "Notifications.Send.NoSender",
                $"No sender configured for notification type {request.Type}"));
        }

        // Try to send
        try
        {
            notification.MarkAsSending();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await sender.SendAsync(
                request.Recipient,
                request.Subject,
                request.Content,
                cancellationToken);

            notification.MarkAsSent();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Publish integration event
            await _eventBus.PublishAsync(new NotificationSentIntegrationEvent
            {
                NotificationId = notification.Id,
                TenantId = notification.TenantId,
                Type = notification.Type.ToString(),
                Recipient = notification.Recipient,
                SentAt = notification.SentAt!.Value
            }, cancellationToken);

            return Result.Success(notification.Id);
        }
        catch (Exception ex)
        {
            notification.MarkAsFailed(ex.Message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<Guid>(Error.Failure(
                "Notifications.Send.Failed",
                $"Failed to send notification: {ex.Message}"));
        }
    }
}

public static class Permissions
{
    public static class Notifications
    {
        public const string View = "notifications.view";
        public const string Send = "notifications.send";
    }
}
