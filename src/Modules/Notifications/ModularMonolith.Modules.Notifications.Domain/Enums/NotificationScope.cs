namespace ModularMonolith.Modules.Notifications.Domain.Enums;

/// <summary>
/// Defines the scope/audience of a notification.
/// </summary>
public enum NotificationScope
{
    /// <summary>
    /// Notification sent to specific users only.
    /// </summary>
    Personal = 0,

    /// <summary>
    /// Notification broadcast to all users within the sender's tenant.
    /// </summary>
    Tenant = 1,

    /// <summary>
    /// Notification broadcast to all users in selected tenants (admin only).
    /// </summary>
    SelectedTenants = 2,

    /// <summary>
    /// Notification broadcast to all users across all tenants (admin only).
    /// </summary>
    AllTenants = 3
}
