using MediatR;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Shared.Abstractions.Messaging;
using ModularMonolith.Shared.Abstractions.Persistence;
using ModularMonolith.Modules.Customers.Domain.Repositories;

namespace ModularMonolith.Modules.Customers.Application.Commands.DeleteCustomer;

internal sealed class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IEventBus _eventBus;

    public DeleteCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IEventBus eventBus)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Customers.Delete))
        {
            return Result.Failure(Error.Forbidden(
                "Customers.Delete.Forbidden",
                "You don't have permission to delete customers"));
        }

        // Get customer
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            return Result.Failure(Error.NotFound(
                "Customers.Delete.NotFound",
                $"Customer with ID '{request.CustomerId}' was not found"));
        }

        // Soft delete
        customer.Delete();

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(new Contracts.Events.CustomerDeletedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            CustomerId = customer.Id,
            TenantId = customer.TenantId
        }, cancellationToken);

        return Result.Success();
    }
}
