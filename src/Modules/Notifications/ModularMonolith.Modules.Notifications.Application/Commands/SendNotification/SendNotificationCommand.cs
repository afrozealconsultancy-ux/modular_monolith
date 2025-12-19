using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Shared.Abstractions.CQRS;

namespace ModularMonolith.Modules.Notifications.Application.Commands.SendNotification;

public sealed record SendNotificationCommand(
    NotificationType Type,
    string Recipient,
    string? Subject,
    string Content) : ICommand<Result<Guid>>;
