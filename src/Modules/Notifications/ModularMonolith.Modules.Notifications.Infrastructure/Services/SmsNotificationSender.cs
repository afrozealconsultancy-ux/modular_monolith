using Microsoft.Extensions.Logging;
using ModularMonolith.Modules.Notifications.Application.Services;
using ModularMonolith.Modules.Notifications.Domain.Enums;

namespace ModularMonolith.Modules.Notifications.Infrastructure.Services;

/// <summary>
/// Stub implementation of SMS notification sender.
/// TODO: Implement actual SMS service integration (e.g., Twilio, AWS SNS)
/// </summary>
internal sealed class SmsNotificationSender : INotificationSender
{
    private readonly ILogger<SmsNotificationSender> _logger;

    public SmsNotificationSender(ILogger<SmsNotificationSender> logger)
    {
        _logger = logger;
    }

    public NotificationType Type => NotificationType.Sms;

    public Task SendAsync(string recipient, string? subject, string content, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual SMS sending
        _logger.LogInformation(
            "Sending SMS to {Recipient} with content '{Content}' (stub implementation)",
            recipient,
            content);

        return Task.CompletedTask;
    }
}
