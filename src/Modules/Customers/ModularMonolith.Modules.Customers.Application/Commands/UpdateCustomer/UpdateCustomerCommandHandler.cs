using MediatR;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Shared.Abstractions.Messaging;
using ModularMonolith.Shared.Abstractions.Persistence;
using ModularMonolith.Modules.Customers.Domain.Exceptions;
using ModularMonolith.Modules.Customers.Domain.Repositories;

namespace ModularMonolith.Modules.Customers.Application.Commands.UpdateCustomer;

internal sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IEventBus _eventBus;

    public UpdateCustomerCommandHandler(
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

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Customers.Update))
        {
            return Result.Failure(Error.Forbidden(
                "Customers.Update.Forbidden",
                "You don't have permission to update customers"));
        }

        // Get customer (automatically filtered by tenant)
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            return Result.Failure(Error.NotFound(
                "Customers.Update.NotFound",
                $"Customer with ID '{request.CustomerId}' was not found"));
        }

        // Update
        customer.Update(request.Name, request.Email, request.PhoneNumber);

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(new Contracts.Events.CustomerUpdatedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            CustomerId = customer.Id,
            TenantId = customer.TenantId,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber
        }, cancellationToken);

        return Result.Success();
    }
}
