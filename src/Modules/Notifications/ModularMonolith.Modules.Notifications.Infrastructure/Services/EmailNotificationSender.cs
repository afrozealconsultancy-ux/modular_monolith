using Microsoft.Extensions.Logging;
using ModularMonolith.Modules.Notifications.Application.Services;
using ModularMonolith.Modules.Notifications.Domain.Enums;

namespace ModularMonolith.Modules.Notifications.Infrastructure.Services;

/// <summary>
/// Stub implementation of email notification sender.
/// TODO: Implement actual SMTP or email service integration (e.g., SendGrid, AWS SES)
/// </summary>
internal sealed class EmailNotificationSender : INotificationSender
{
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(ILogger<EmailNotificationSender> logger)
    {
        _logger = logger;
    }

    public NotificationType Type => NotificationType.Email;

    public Task SendAsync(string recipient, string? subject, string content, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual email sending
        _logger.LogInformation(
            "Sending email to {Recipient} with subject '{Subject}' (stub implementation)",
            recipient,
            subject);

        return Task.CompletedTask;
    }
}
