using ModularMonolith.Shared.Abstractions.CQRS;

namespace ModularMonolith.Modules.Notifications.Application.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId) : ICommand<Result>;
