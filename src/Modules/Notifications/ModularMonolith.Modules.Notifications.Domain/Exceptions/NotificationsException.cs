namespace ModularMonolith.Modules.Notifications.Domain.Exceptions;

public abstract class NotificationsException : Exception
{
    protected NotificationsException(string message) : base(message)
    {
    }
}

public sealed class NotificationNotFoundException : NotificationsException
{
    public NotificationNotFoundException(Guid notificationId)
        : base($"Notification with ID '{notificationId}' was not found")
    {
        NotificationId = notificationId;
    }

    public Guid NotificationId { get; }
}

public sealed class InvalidNotificationRecipientException : NotificationsException
{
    public InvalidNotificationRecipientException(string recipient, string reason)
        : base($"Invalid notification recipient '{recipient}': {reason}")
    {
        Recipient = recipient;
    }

    public string Recipient { get; }
}
