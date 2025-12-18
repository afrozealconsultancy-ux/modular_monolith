using MediatR;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Modules.Customers.Domain.Repositories;

namespace ModularMonolith.Modules.Customers.Application.Queries.GetCustomer;

internal sealed class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, Result<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUser _currentUser;

    public GetCustomerQueryHandler(
        ICustomerRepository customerRepository,
        ICurrentUser currentUser)
    {
        _customerRepository = customerRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Customers.View))
        {
            return Result.Failure<CustomerDto>(Error.Forbidden(
                "Customers.View.Forbidden",
                "You don't have permission to view customers"));
        }

        // Get customer (automatically filtered by tenant)
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            return Result.Failure<CustomerDto>(Error.NotFound(
                "Customers.View.NotFound",
                $"Customer with ID '{request.CustomerId}' was not found"));
        }

        var dto = new CustomerDto(
            customer.Id,
            customer.TenantId,
            customer.Name,
            customer.Email,
            customer.PhoneNumber,
            (int)customer.Status,
            customer.CreatedAt,
            customer.UpdatedAt,
            customer.Addresses.Select(a => new CustomerAddressDto(
                a.Id,
                a.Street,
                a.City,
                a.State,
                a.PostalCode,
                a.Country,
                (int)a.Type
            )).ToList());

        return Result.Success(dto);
    }
}
