using ModularMonolith.Modules.Notifications.Domain.Entities;
using ModularMonolith.Shared.Abstractions.Persistence;

namespace ModularMonolith.Modules.Notifications.Domain.Repositories;

public interface INotificationRepository : IRepository<Notification, Guid>
{
    Task<IReadOnlyList<Notification>> GetPendingNotificationsAsync(int limit, CancellationToken cancellationToken = default);
}
