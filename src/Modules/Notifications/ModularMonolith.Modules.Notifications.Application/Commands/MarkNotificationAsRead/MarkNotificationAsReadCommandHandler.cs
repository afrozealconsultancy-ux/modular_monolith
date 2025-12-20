using MediatR;
using ModularMonolith.Modules.Notifications.Domain.Repositories;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.CQRS;
using ModularMonolith.Shared.Abstractions.Kernel;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Notifications.Application.Commands.MarkNotificationAsRead;

internal sealed class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly INotificationRecipientRepository _recipientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;

    public MarkNotificationAsReadCommandHandler(
        INotificationRecipientRepository recipientRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ITenantContext tenantContext)
    {
        _recipientRepository = recipientRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant)
        {
            return Result.Failure(Error.Unauthorized(
                "Notifications.MarkAsRead.NoTenant",
                "Tenant context is required"));
        }

        // Find the recipient record for this user
        var recipient = await _recipientRepository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (recipient == null)
        {
            return Result.Failure(Error.NotFound(
                "Notifications.MarkAsRead.NotFound",
                "Notification not found"));
        }

        // Verify this recipient belongs to the current user
        if (recipient.UserId != _currentUser.UserId || recipient.TenantId != _tenantContext.TenantId!.Value)
        {
            return Result.Failure(Error.Forbidden(
                "Notifications.MarkAsRead.Forbidden",
                "You can only mark your own notifications as read"));
        }

        try
        {
            recipient.MarkAsRead();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure(
                "Notifications.MarkAsRead.Failed",
                $"Failed to mark notification as read: {ex.Message}"));
        }
    }
}
