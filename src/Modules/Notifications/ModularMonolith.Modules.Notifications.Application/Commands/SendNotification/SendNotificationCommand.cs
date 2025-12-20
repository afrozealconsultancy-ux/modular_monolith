using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Shared.Abstractions.CQRS;

namespace ModularMonolith.Modules.Notifications.Application.Commands.SendNotification;

public sealed record SendNotificationCommand(
    NotificationScope Scope,
    NotificationType Type,
    string? Subject,
    string Content,
    List<Guid>? TargetTenantIds,        // For SelectedTenants scope
    List<Guid>? TargetUserIds,          // For Personal scope
    DateTime? ScheduledFor) : ICommand<Result<Guid>>;
