using ModularMonolith.Modules.Notifications.Domain.Enums;

namespace ModularMonolith.Modules.Notifications.Application.Services;

public interface INotificationSender
{
    NotificationType Type { get; }
    Task SendAsync(string recipient, string? subject, string content, CancellationToken cancellationToken = default);
}
