using MediatR;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Shared.Abstractions.Messaging;
using ModularMonolith.Shared.Abstractions.Persistence;
using ModularMonolith.Shared.Abstractions.Tenancy;
using ModularMonolith.Modules.Customers.Domain.Entities;
using ModularMonolith.Modules.Customers.Domain.Exceptions;
using ModularMonolith.Modules.Customers.Domain.Repositories;
using ModularMonolith.Modules.Customers.Domain.ValueObjects;

namespace ModularMonolith.Modules.Customers.Application.Commands.CreateCustomer;

internal sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IEventBus _eventBus;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IEventBus eventBus)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _eventBus = eventBus;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Customers.Create))
        {
            return Result.Failure<Guid>(Error.Forbidden(
                "Customers.Create.Forbidden",
                "You don't have permission to create customers"));
        }

        // Ensure tenant context
        if (!_tenantContext.HasTenant)
        {
            return Result.Failure<Guid>(Error.Unauthorized(
                "Customers.Create.NoTenant",
                "Tenant context is required"));
        }

        // Check if customer with email already exists (within tenant)
        if (await _customerRepository.ExistsWithEmailAsync(request.Email, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict(
                "Customers.Create.EmailExists",
                $"A customer with email '{request.Email}' already exists"));
        }

        // Create customer
        var customer = Customer.Create(
            _tenantContext.TenantId!.Value,
            request.Name,
            request.Email,
            request.PhoneNumber);

        // Add addresses if provided
        if (request.Addresses?.Any() == true)
        {
            foreach (var addressDto in request.Addresses)
            {
                var address = Address.Create(
                    addressDto.Street,
                    addressDto.City,
                    addressDto.State,
                    addressDto.PostalCode,
                    addressDto.Country,
                    (AddressType)addressDto.Type);

                customer.AddAddress(address);
            }
        }

        // Save
        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration event for OTHER modules
        await _eventBus.PublishAsync(new Contracts.Events.CustomerCreatedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            CustomerId = customer.Id,
            TenantId = customer.TenantId,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber
        }, cancellationToken);

        return Result.Success(customer.Id);
    }
}
